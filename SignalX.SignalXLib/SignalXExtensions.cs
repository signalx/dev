﻿namespace SignalXLib.Lib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;

    public static class SignalXExtensions
    {
        [Obsolete("SET signalx.MESSAGE_ID TO USE SPECIFIC MESSAGE ID FOR REQUESTS")]
        public static async Task RespondToScriptRequest(
            this SignalX signalX,
            string correlationId,
            HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groups)
        {
            signalX.Advanced.Trace(correlationId, "Preparing script for client ...");
            signalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.SignalXRequestForMethods.ToString(), context?.User?.Identity?.Name));
            if (!await signalX.CanProcess(correlationId, context, "", null, true).ConfigureAwait(false))
            {
                signalX.Settings.WarningHandler.ForEach(h => h?.Invoke("RequireAuthentication", "User attempting to connect has not been authenticated when authentication is required"));
                return;
            }

            string logRequestOnClient = signalX.Settings.LogAgentMessagesOnClient ? signalX.Settings.ClientLogScriptFunction + "(message);" : "";
            string logResponseOnClient = signalX.Settings.LogAgentMessagesOnClient ? signalX.Settings.ClientLogScriptFunction + "(response);" : "";
            string receiveErrorsFromClient = (signalX.Settings.ReceiveErrorMessagesFromClient
                ? signalX.ClientErrorSnippet
                : "") + (signalX.Settings.ReceiveDebugMessagesFromClient
                ? signalX.ClientDebugSnippet
                : "");
            string clientAgent = @"
                 " + receiveErrorsFromClient + @"
                 signalx.client." + SignalX.SIGNALXCLIENTAGENT + @" = function (message) {
                   " + logRequestOnClient + @"
                   var response ={};
                   try{ response={ Error : '', MessageAsJsonString : JSON.stringify(eval('(function(){ '+message+' })()')) }; }catch(err){  response = {Error : err, MessageAsJsonString :'' }; signalx.error.f({error:err,description:'error occured while evaluating server agent command',context:'server agent error context'});  }
                   " + logResponseOnClient + @"
                    signalx.server." + SignalX.SIGNALXCLIENTAGENT + @"(response,function(messageResponse){  });
                 };";

            string clientReady = signalX.OnClientReady.Count == 0
                ? ""
                : @"
                    signalx.beforeOthersReady=function(f){
                        signalx.debug.f('Checking with server before executing ready functions...')
                        signalx.server." + SignalX.SIGNALXCLIENTREADY + @"('',function(){
                             signalx.debug.f('Server indicates readiness. Now running client ready functions ... ');
                             typeof f ==='function' && f();
                          });
                 }";

            string methods = "; window.signalxidgen=window.signalxidgen||function(){return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {    var r = Math.random()*16|0, v = c == 'x' ? r : (r&0x3|0x8);    return v.toString(16);})};" +
                signalX.SignalXServers.Aggregate(clientReady + clientAgent + "var $sx= {", (current, signalXServer) => current + signalXServer.Key + @":function(m,repTo,sen,msgId){ var deferred = $.Deferred();  window.signalxid=window.signalxid||window.signalxidgen();   sen=sen||window.signalxid;repTo=repTo||''; var messageId=signalx.MESSAGE_ID || window.signalxidgen(); var rt=repTo; if(typeof repTo==='function'){ signalx.waitingList(messageId,repTo);rt=messageId;  }  if(!repTo){ signalx.waitingList(messageId,deferred);rt=messageId;  }  var messageToSend={handler:'" + signalXServer.Key + "',message:m, replyTo:rt,sender:sen, groupList:signalx.groupList};  chat.server.send('" + signalXServer.Key + "',(m && JSON.stringify(m)) ||'',rt,sen,messageId,signalx.groupList||[]); if(repTo){return messageId}else{ return deferred.promise();}   },").Trim()
                + "}; $sx; ";

            if (signalX.Settings.StartCountingInComingMessages)
                Interlocked.Increment(ref signalX.Settings.InComingCounter);

            signalX.Advanced.Trace(correlationId, "Sending script to client ...");
            signalX.Receiver.ReceiveScripts(correlationId, context?.ConnectionId, methods, context, groups, clients);

            signalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.SignalXRequestForMethodsCompleted.ToString(), methods));
        }

        #region METHODS

        internal static void UpdateClient(this SignalX signalX, string correlationId, string clientMethodName, bool status)
        {
            if (string.IsNullOrEmpty(clientMethodName))
                throw new ArgumentNullException(nameof(clientMethodName));
            if (signalX.SignalXClientDetails.ContainsKey(clientMethodName))
                signalX.SignalXClientDetails[clientMethodName].Disabled = status;
            else
                signalX.SignalXClientDetails.GetOrAdd(clientMethodName, new ClientDetails { Disabled = status });
        }

        /// <summary>
        ///     Stops message sending to a given client
        /// </summary>
        public static void DisableClient(this SignalX signalX, string clientMethodName)
        {
            string correlationId = Guid.NewGuid().ToString();
            signalX.Advanced.Trace(correlationId, $"Disabling client {clientMethodName}...");
            UpdateClient(signalX, correlationId, clientMethodName, true);
        }

        public static void DisableAllClients(this SignalX signalX, string correlationId = null)
        {
            correlationId = correlationId ?? Guid.NewGuid().ToString();
            signalX.Advanced.Trace(correlationId, "Disabling all clients ...");

            signalX.Settings.DisabledAllClients = true;
            foreach (string s in signalX.SignalXClientDetails.Select(x => x.Key).ToList())
                signalX.UpdateClient(correlationId, s, true);
        }

        /// <summary>
        ///     This just turns around to set the GlobalHost.Configuration.DefaultMessageBufferSize
        ///     Since messages are stored in the message bus in server memory, reducing the size of messages can also address
        ///     server memory issues.
        ///     Since SignalR doesn't block when you call client methods, you can invoke client methods very quickly.
        ///     Unfortunately, the client might not always be ready to receive messages immediately once you send them, so SignalR
        ///     has to buffer messages
        ///     So by default SignalR will buffer up to 1000 messages per client.
        ///     Once the client falls behind by over 1000 messages, it will start missing messages
        ///     By default, SignalR retains 1000 messages in memory per hub per connection.
        ///     If large messages are being used, this may create memory issues which can be
        ///     alleviated by reducing this value. This setting can be set in the
        ///     Application_Start event handler in an ASP.NET application, or in the
        ///     Configuration method of an OWIN startup class in a self-hosted application
        ///     see https://docs.microsoft.com/en-us/aspnet/signalr/overview/performance/signalr-performance#tuning
        /// </summary>
        /// <param name="defaultMessageBufferSize">Default val is 1000 </param>
        public static void SetGlobalDefaultMessageBufferSize(this SignalX signalX, int defaultMessageBufferSize = 1000)
        {
            signalX.Advanced.Trace(signalX.AppCorrelationId, "Setting global message buffer size ...");
            GlobalHost.Configuration.DefaultMessageBufferSize = defaultMessageBufferSize;
        }

        public static void EnableClient(this SignalX signalX, string clientMethodName)
        {
            signalX.Advanced.Trace(signalX.AppCorrelationId, $"Enabling client {clientMethodName}...");
            signalX.UpdateClient(signalX.AppCorrelationId, clientMethodName, false);
        }

        public static void EnableAllClients(this SignalX signalX)
        {
            signalX.Advanced.Trace(signalX.AppCorrelationId, "Enabling all clients ...");

            signalX.Settings.DisabledAllClients = false;
            foreach (string s in signalX.SignalXClientDetails.Select(x => x.Key).ToList())
                signalX.UpdateClient(signalX.AppCorrelationId, s, false);
        }

        internal static async Task<bool> CanProcess(this SignalX signalX, string correlationId, HubCallerContext context, string serverHandlerName, SignalXRequest request, bool isScriptRequest)
        {
            signalX.Advanced.Trace(correlationId, $"Checking if request can be processed for {serverHandlerName}...");
            bool result = false;

            if (isScriptRequest)
            {
                signalX.Advanced.Trace(correlationId, $"Its a script request so no further checks are necessary for {serverHandlerName}...");
                return true;
            }

            if (signalX.SignalXServerExecutionDetails.ContainsKey(serverHandlerName))
            {
                List<string> allowedGroups = signalX.SignalXServerExecutionDetails[serverHandlerName].AllowedGroups;
                signalX.Advanced.Trace(correlationId, $"Checking if request is coming from a client with group allowed to access server {serverHandlerName}...");
                foreach (string allowedGroup in allowedGroups)
                    if (!request.Groups.Contains(allowedGroup))
                    {
                        signalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.SignalXRequestAuthorizationFailed.ToString(), $"Authorization failed : The request does not contain group {allowedGroup} and is therefore denied access to {serverHandlerName}"));
                        return false;
                    }
            }

            if (signalX.Settings.RequireAuthorizationForAllHandlers ||
                signalX.SignalXServerExecutionDetails.ContainsKey(serverHandlerName) &&
                signalX.SignalXServerExecutionDetails[serverHandlerName].RequiresAuthorization)
            {
                signalX.Advanced.Trace(correlationId, $"Checking if request is authorized to access the server {serverHandlerName} because authorization function has been set ...");

                if (signalX.Settings.AuthenticatedWhen != null)
                {
                    result = await signalX.IsAuthenticated(correlationId, context.Request, request).ConfigureAwait(false);
                }
                else
                {
                    result = context.User.Identity.IsAuthenticated;
                    if (!result)
                        signalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.SignalXRequestAuthorizationFailed.ToString(), "Authorization failed after checking with context.User.Identity.IsAuthenticated. Custom Authorization check is not yet setup "));
                }
            }
            else
            {
                signalX.Settings.WarningHandler.ForEach(h => h?.Invoke("AuthorizationNotSet", "Try setting an authorization handler to prevent anonymous access into your server"));
                result = true;
            }

            signalX.Advanced.Trace(correlationId, $"request allowed to access the server {serverHandlerName} : {request}");

            return result;
        }

        internal static async Task<bool> IsAuthenticated(this SignalX signalX, string correlationId, IRequest request, SignalXRequest sRequest)
        {
            bool result;
            try
            {
                //var cookie = request?.Cookies[AuthenticationCookieName];
                //var ip = request?.Environment["server.RemoteIpAddress"]?.ToString();
                result = await signalX.Settings.AuthenticatedWhen(sRequest).ConfigureAwait(false);
                if (!result)
                    signalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.SignalXRequestAuthorizationFailed.ToString(), "Authorization failed after checking with Custom Authorization provided"));
            }
            catch (Exception e)
            {
                result = false;
                signalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.SignalXRequestAuthorizationFailed.ToString(), "Custom Authorization threw an exception " + e.Message));
                signalX.Settings.ExceptionHandler.ForEach(h => h?.Invoke("Authentication failed", e));
            }

            return result;
        }

        /// <summary>
        ///     Cookie authentication supported
        /// </summary>
        //public static void AuthenticationHandler(string cookieName, Func<Cookie,string, string, IRequest, bool> handler)
        //{
        //    AuthenticationCookieName = cookieName ?? throw new ArgumentNullException(nameof(cookieName));
        //    AuthenticatedWhen = handler ?? throw new ArgumentNullException(nameof(handler));
        //}
        public static void AuthenticationHandler(this SignalX signalX, Func<SignalXRequest, Task<bool>> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            signalX.Advanced.Trace(signalX.AppCorrelationId, "Setting up  AuthenticationHandler ...");

            signalX.Settings.AuthenticationCookieName = null;
            signalX.Settings.AuthenticatedWhen = handler;
        }

        /// <summary>
        ///     Adds a warning handler
        /// </summary>
        /// <param name="signalX"></param>
        /// <param name="handler"></param>
        public static void OnWarning(this SignalX signalX, Action<string, object> handler)
        {
            signalX.Settings.WarningHandler.Add(
                (s, o) =>
                {
                    try
                    {
                        handler(s, o);
                    }
                    catch (Exception e)
                    {
                        //todo swallow
                    }
                });
        }

        /// <summary>
        ///     Adds an exception handler
        /// </summary>
        /// <param name="signalX"></param>
        /// <param name="handler"></param>
        public static void OnException(this SignalX signalX, Action<Exception> handler)
        {
            signalX.Settings.ExceptionHandler.Add(
                (m, e) =>
                {
                    try
                    {
                        handler.Invoke(e);
                    }
                    catch (Exception exception)
                    {
                        //todo swallow
                    }
                });
        }

        /// <summary>
        ///     Adds an exception handler
        /// </summary>
        /// <param name="signalX"></param>
        /// <param name="handler"></param>
        public static void OnException(this SignalX signalX, Action<string, Exception> handler)
        {
            signalX.Settings.ExceptionHandler.Add(
                (s, o) =>
                {
                    try
                    {
                        handler(s, o);
                    }
                    catch (Exception e)
                    {
                        //todo swallow
                    }
                });
        }

        /// <summary>
        ///     Adds a connection event handler
        /// </summary>
        /// <param name="signalX"></param>
        /// <param name="handler"></param>
        public static void OnConnectionEvent(this SignalX signalX, Action<string, object> handler)
        {
            signalX.Advanced.Trace(signalX.AppCorrelationId, "Setting up  OnConnectionEvent ...");
            signalX.Settings.ConnectionEventsHandler.Add(handler);
        }

        public static async Task<double> GetOutGoingMessageSpeedAsync(this SignalX signalX, TimeSpan duration)
        {
            signalX.Settings.OutGoingCounter = 0;
            var sw = new Stopwatch();
            signalX.Settings.StartCountingOutGoingMessages = true;
            sw.Start();
            await Task.Delay(duration); // might not be 5 sec
            signalX.Settings.StartCountingOutGoingMessages = false;
            sw.Stop();
            long currentCount = Interlocked.Read(ref signalX.Settings.OutGoingCounter);
            return currentCount / sw.Elapsed.TotalSeconds;
        }

        public static async Task<double> GetInComingMessageSpeedAsync(this SignalX signalX, TimeSpan duration)
        {
            signalX.Settings.InComingCounter = 0;
            var sw = new Stopwatch();
            signalX.Settings.StartCountingInComingMessages = true;
            sw.Start();
            await Task.Delay(duration); // might not be 5 sec
            signalX.Settings.StartCountingInComingMessages = false;
            sw.Stop();
            long currentCount = Interlocked.Read(ref signalX.Settings.InComingCounter);
            return currentCount / sw.Elapsed.TotalSeconds;
        }

        /// <summary>
        ///     Reply to a specific client
        /// </summary>
        /// <param name="signalX"></param>
        /// <param name="user"></param>
        /// <param name="name"></param>
        /// <param name="data"></param>
        public static void RespondToUser(this SignalX signalX, string user, string name, object data, string correlationId = null)
        {
            correlationId = correlationId ?? Guid.NewGuid().ToString();
            if (!signalX.AllowToSend(name, data, correlationId))
                return;
            if (signalX.Settings.StartCountingOutGoingMessages)
                Interlocked.Increment(ref signalX.Settings.OutGoingCounter);

            signalX.Receiver.Receive(user, name, data, correlationId);
        }

        /// <summary>
        ///     Reply to every other clients but the sender
        /// </summary>
        /// <param name="signalX"></param>
        /// <param name="excludedUserId"></param>
        /// <param name="name"></param>
        /// <param name="data"></param>
        /// <param name="groupName"></param>
        public static void RespondToOthers(this SignalX signalX, string excludedUserId, string name, object data, string groupName = null, string correlationId = null)
        {
            if (!signalX.AllowToSend(name, data, correlationId))
                return;
            if (signalX.Settings.StartCountingOutGoingMessages)
                Interlocked.Increment(ref signalX.Settings.OutGoingCounter);

            signalX.Receiver.ReceiveAsOther(correlationId, name, data, excludedUserId, groupName);
        }

        internal static async Task CallServer(this SignalX signalX, SignalXRequest request, string correlationId)
        {
            signalX.Advanced.Trace(correlationId, $"Running call to server {request?.Handler}...");
            ServerHandlerDetails executionDetails = signalX.SignalXServerExecutionDetails[request.Handler];
            if (executionDetails.IsSingleWriter)
                using (executionDetails.SingleWriter.Write())
                {
                    await signalX.SignalXServers[request.Handler].Invoke(request, signalX.SignalXServerExecutionDetails[request.Handler].State).ConfigureAwait(false);
                }
            else
                await signalX.SignalXServers[request.Handler].Invoke(request, signalX.SignalXServerExecutionDetails[request.Handler].State).ConfigureAwait(false);
        }

        public static void RunJavaScriptOnAllClients(this SignalX signalX, string script, Action<ResponseAfterScriptRuns> onResponse = null, TimeSpan? delay = null)
        {
            signalX.RunJavaScriptOnAllClientsInGroup(script, null, onResponse, delay);
        }

        public static void RunJavaScriptOnAllClientsInGroup(this SignalX signalX, string script, string groupName, Action<ResponseAfterScriptRuns> onResponse = null, TimeSpan? delay = null)
        {
            signalX.Advanced.Trace(signalX.AppCorrelationId, $"Running javascript on all clients in group {groupName} ..." + script);

            if (signalX.OnResponseAfterScriptRuns != null)
            {
                //todo do something here to maybe block multiple calls
                //todo before a previous one finishes
                //todo or throw a warning
            }

            delay = delay ?? TimeSpan.FromSeconds(0);
            signalX.OnResponseAfterScriptRuns = onResponse;
            Task.Delay(delay.Value).ContinueWith(
                c => { signalX.RespondToAllInGroup(SignalX.SIGNALXCLIENTAGENT, script, groupName, signalX.AppCorrelationId); });
        }

        /// <summary>
        ///     Runs when client is ready before client's ready functions executes
        /// </summary>
        /// <param name="signalX"></param>
        /// <param name="onResponse"></param>
        public static void OnClientReady(this SignalX signalX, Action<SignalXRequest> onResponse)
        {
            signalX.Advanced.Trace(signalX.AppCorrelationId, "Setting up  OnClientReady ...");
            signalX.OnClientReady.Add(onResponse);
        }

        /// <summary>
        ///     Runs when client is ready before client's ready functions executes
        /// </summary>
        /// <param name="signalX"></param>
        /// <param name="onResponse"></param>
        public static void OnClientReady(this SignalX signalX, Action onResponse)
        {
            if (onResponse != null)
            {
                signalX.Advanced.Trace(signalX.AppCorrelationId, "Setting up  OnClientReady ...");
                signalX.OnClientReady.Add(r => { onResponse?.Invoke(); });
            }
        }

        public static void RunJavaScriptOnUser(this SignalX signalX, string userId, string script, Action<ResponseAfterScriptRuns> onResponse = null, TimeSpan? delay = null)
        {
            signalX.Advanced.Trace(signalX.AppCorrelationId, $"Running javascript on user {userId}  ..." + script);

            if (signalX.OnResponseAfterScriptRuns != null)
            {
                //todo do something here to maybe block multiple calls
                //todo before a previous one finishes
                //todo or throw a warning
            }

            delay = delay ?? TimeSpan.FromSeconds(0);
            signalX.OnResponseAfterScriptRuns = onResponse;
            Task.Delay(delay.Value).ContinueWith(c => { signalX.RespondToUser(userId, SignalX.SIGNALXCLIENTAGENT, script, signalX.AppCorrelationId); });
        }

        internal static void JoinGroup(
            this SignalX signalX,
            string correlationId,
            HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groups,
            string groupName)
        {
            signalX.Advanced.Trace(correlationId, $"User {context?.ConnectionId} is joining group {groupName}...");

            groups.Add(context?.ConnectionId, groupName);
            signalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.SignalXGroupJoin.ToString(), groupName));

            signalX.Receiver.ReceiveInGroupManager(correlationId, "join", context?.ConnectionId, groupName, context, clients, groups);
        }

        internal static void LeaveGroup(
            this SignalX signalX,
            string correlationId,
            HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groups,
            string groupName)
        {
            signalX.Advanced.Trace(correlationId, $"User {context?.ConnectionId} is leaving group {groupName}...");
            groups.Remove(context?.ConnectionId, groupName);
            signalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.SignalXGroupLeave.ToString(), groupName));
            signalX.Receiver.ReceiveInGroupManager(correlationId, "leave", context?.ConnectionId, groupName, context, clients, groups);
        }

        public static string GenerateUniqueNameId()
        {
            return "S" + Guid.NewGuid().ToString().Replace("-", "");
        }

        #endregion METHODS

        #region ServerDefinitions

        /// <summary>
        ///     Sets up a server
        /// </summary>
        /// <param name="signalX"></param>
        /// <param name="serverType"></param>
        /// <param name="name">A unique name for the server, unless dynamic server is allowed</param>
        /// <param name="server"></param>
        /// <param name="groupNames"></param>
        /// <param name="requireAuthorization">
        ///     Indicates if the set authorization should be checked before allowing access to the
        ///     server
        /// </param>
        /// <param name="isSingleWriter">Set whether or not requests should be queued and only one allowed in at a time</param>
        /// <param name="allowDynamicServerForThisInstance">
        ///     Sets if it is allowed for another server to be created with same name.
        ///     In such a case, the existing server will be discarded
        /// </param>
        internal static void ServerBase(
            this SignalX signalX,
            ServerType serverType,
            string name,
            Func<SignalXRequest, SignalXServerState, Task> server,
            List<string> groupNames = null
        )
        {
            bool requireAuthorization = false;
            bool isSingleWriter = false;
            bool allowDynamicServerForThisInstance = false;

            switch (serverType)
            {
                case ServerType.Default:
                    break;

                case ServerType.SingleAccess:
                    isSingleWriter = true;
                    break;

                case ServerType.Authorized:
                    requireAuthorization = true;
                    break;

                case ServerType.AuthorizedSingleAccess:
                    requireAuthorization = true;
                    isSingleWriter = true;
                    break;

                case ServerType.Dynamic:
                    allowDynamicServerForThisInstance = true;
                    break;

                case ServerType.DynamicAuthorized:
                    allowDynamicServerForThisInstance = true;
                    requireAuthorization = true;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(serverType), serverType, null);
            }

            signalX.Advanced.Trace(signalX.AppCorrelationId, $"Creating a server {name} with authorized groups {string.Join(",", groupNames ?? new List<string>())} and requires authorization : {requireAuthorization}, is set to single write : {isSingleWriter} and allows dynamic server for this instance {allowDynamicServerForThisInstance}");

            groupNames = groupNames ?? new List<string>();
            name = name.Trim();
            string camelCased = char.ToLowerInvariant(name[0]) + name.Substring(1);
            string unCamelCased = char.ToUpperInvariant(name[0]) + name.Substring(1);

            if ((signalX.SignalXServers.ContainsKey(camelCased) || signalX.SignalXServers.ContainsKey(unCamelCased)) && !signalX.Settings.AllowDynamicServerInternal && !allowDynamicServerForThisInstance)
            {
                var exc = new Exception("Server with name '" + name + "' has already been created");
                signalX.Advanced.Trace(signalX.AppCorrelationId, exc.Message, exc);

                throw exc;
            }

            try
            {
                if (signalX.SignalXServers.ContainsKey(camelCased))
                {
                    signalX.SignalXServers[camelCased] = server;
                    signalX.SignalXServerExecutionDetails[camelCased] = new ServerHandlerDetails(requireAuthorization, isSingleWriter, groupNames);
                    // signalX.Server.GetOrAdd(camelCased, signalX.RespondToServer);
                    if (camelCased != unCamelCased)
                    {
                        signalX.SignalXServers[unCamelCased] = server;
                        signalX.SignalXServerExecutionDetails[unCamelCased] = new ServerHandlerDetails(requireAuthorization, isSingleWriter, groupNames);
                        //  signalX.Server.GetOrAdd(unCamelCased, signalX.RespondToServer);
                    }
                }
                else
                {
                    signalX.SignalXServers.GetOrAdd(camelCased, server);
                    signalX.SignalXServerExecutionDetails.GetOrAdd(camelCased, new ServerHandlerDetails(requireAuthorization, isSingleWriter, groupNames));
                    // signalX.Server.GetOrAdd(camelCased, signalX. RespondToServer);
                    if (camelCased != unCamelCased)
                    {
                        signalX.SignalXServers.GetOrAdd(unCamelCased, server);
                        signalX.SignalXServerExecutionDetails.GetOrAdd(unCamelCased, new ServerHandlerDetails(requireAuthorization, isSingleWriter, groupNames));
                        //  signalX.Server.GetOrAdd(unCamelCased, signalX.RespondToServer);
                    }
                }
            }
            catch (Exception e)
            {
                signalX.Advanced.Trace(signalX.AppCorrelationId, $"Error creating a server {name} with authorized groups {string.Join(",", groupNames ?? new List<string>())} and requires authorization : {requireAuthorization}, is set to single write : {isSingleWriter} and allows dynamic server for this instance {allowDynamicServerForThisInstance}", e);

                signalX.Settings.ExceptionHandler.ForEach(h => h?.Invoke($"Error while creating server {name}", e));
            }
        }

        public static void ServerAsync(this SignalX signalX, string name, Func<SignalXRequest, SignalXServerState, Task> server, List<string> groupNames = null)
        {
            signalX.ServerBase(ServerType.Default, name, server, groupNames);
        }

        public static void ServerAsync(this SignalX signalX, string name, Func<SignalXRequest, Task> server, List<string> groupNames = null)
        {
            signalX.ServerBase(ServerType.Default, name, (r, s) => server(r), groupNames);
        }

        public static void ServerAsync(this SignalX signalX, ServerType serverType, string name, Func<SignalXRequest, SignalXServerState, Task> server, List<string> groupNames = null)
        {
            signalX.ServerBase(serverType, name, server, groupNames);
        }

        public static void ServerAsync(this SignalX signalX, ServerType serverType, string name, Func<SignalXRequest, Task> server, List<string> groupNames = null)
        {
            signalX.ServerBase(serverType, name, (r, s) => server(r), groupNames);
        }

        public static void Server(this SignalX signalX, string name, Action<SignalXRequest, SignalXServerState> server, List<string> groupNames = null)
        {
            signalX.ServerBase(
                ServerType.Default,
                name,
                (r, s) =>
                {
                    server(r, s);
                    return Task.FromResult(true);
                },
                groupNames);
        }

        public static void Server(this SignalX signalX, string name, Action<SignalXRequest> server, List<string> groupNames = null)
        {
            signalX.ServerBase(
                ServerType.Default,
                name,
                (r, s) =>
                {
                    server(r);
                    return Task.FromResult(true);
                },
                groupNames);
        }

        public static void Server(this SignalX signalX, ServerType serverType, string name, Action<SignalXRequest, SignalXServerState> server, List<string> groupNames = null)
        {
            signalX.ServerBase(
                serverType,
                name,
                (r, s) =>
                {
                    server(r, s);
                    return Task.FromResult(true);
                },
                groupNames);
        }

        public static void Server(this SignalX signalX, ServerType serverType, string name, Action<SignalXRequest> server, List<string> groupNames = null)
        {
            signalX.ServerBase(
                serverType,
                name,
                (r, s) =>
                {
                    server(r);
                    return Task.FromResult(true);
                },
                groupNames);
        }

        #endregion ServerDefinitions
    }
}