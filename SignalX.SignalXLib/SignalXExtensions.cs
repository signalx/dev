namespace SignalXLib.Lib
{
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public static class SignalXExtensions
    {
        #region METHODS

        internal static void UpdateClient(this SignalX signalX, string clientMethodName, bool status)
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
            signalX.Advanced.Trace($"Disabling client {clientMethodName}...");
            UpdateClient(signalX, clientMethodName, true);
        }

        public static void DisableAllClients(this SignalX signalX)
        {
            signalX.Advanced.Trace($"Disabling all clients ...");

            signalX.Settings.DisabledAllClients = true;
            foreach (string s in signalX.SignalXClientDetails.Select(x => x.Key).ToList())
                signalX.UpdateClient(s, true);
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
            signalX.Advanced.Trace($"Setting global message buffer size ...");
            GlobalHost.Configuration.DefaultMessageBufferSize = defaultMessageBufferSize;
        }

        public static void EnableClient(this SignalX signalX, string clientMethodName)
        {
            signalX.Advanced.Trace($"Enabling client {clientMethodName}...");
            signalX.UpdateClient(clientMethodName, false);
        }

        public static void EnableAllClients(this SignalX signalX)
        {
            signalX.Advanced.Trace($"Enabling all clients ...");

            signalX.Settings.DisabledAllClients = false;
            foreach (string s in signalX.SignalXClientDetails.Select(x => x.Key).ToList())
                signalX.UpdateClient(s, false);
        }

        internal static bool CanProcess(this SignalX signalX, HubCallerContext context, string serverHandlerName, SignalXRequest request, bool isScriptRequest)
        {
            signalX.Advanced.Trace($"Checking if request can be processed for {serverHandlerName}...");
            bool result = false;

            if (isScriptRequest)
            {
                signalX.Advanced.Trace($"Its a script request so no further checks are necessary for {serverHandlerName}...");
                return true;
            }

            if (signalX.SignalXServerExecutionDetails.ContainsKey(serverHandlerName))
            {
                var allowedGroups = signalX.SignalXServerExecutionDetails[serverHandlerName].AllowedGroups;
                signalX.Advanced.Trace($"Checking if request is coming from a client with group allowed to access server {serverHandlerName}...");
                foreach (var allowedGroup in allowedGroups)
                {
                    if (!request.Groups.Contains(allowedGroup))
                    {
                        signalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.SignalXRequestAuthorizationFailed.ToString(), $"Authorization failed : The request does not contain group {allowedGroup} and is therefore denied access to {serverHandlerName}"));
                        return false;
                    }
                }
            }

            if (signalX.Settings.RequireAuthorizationForAllHandlers ||
                signalX.SignalXServerExecutionDetails.ContainsKey(serverHandlerName) &&
                signalX.SignalXServerExecutionDetails[serverHandlerName].RequiresAuthorization)
            {
                signalX.Advanced.Trace($"Checking if request is authorized to access the server {serverHandlerName} because authorization function has been set ...");

                if (signalX.Settings.AuthenticatedWhen != null)
                {
                    result = signalX.IsAuthenticated(context.Request, request);
                }
                else
                {
                    result = context.User.Identity.IsAuthenticated;
                    if (!result)
                        signalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.SignalXRequestAuthorizationFailed.ToString(), $"Authorization failed after checking with context.User.Identity.IsAuthenticated. Custom Authorization check is not yet setup "));
                }
            }
            else
            {
                signalX.Settings.WarningHandler.ForEach(h => h?.Invoke("AuthorizationNotSet", "Try setting an authorization handler to prevent anonymous access into your server"));
                result = true;
            }
            signalX.Advanced.Trace($"request allowed to access the server {serverHandlerName} : {request}");

            return result;
        }

        internal static bool IsAuthenticated(this SignalX signalX, IRequest request, SignalXRequest sRequest)
        {
            bool result;
            try
            {
                //var cookie = request?.Cookies[AuthenticationCookieName];
                //var ip = request?.Environment["server.RemoteIpAddress"]?.ToString();
                result = signalX.Settings.AuthenticatedWhen(sRequest);
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
        public static void AuthenticationHandler(this SignalX signalX, Func<SignalXRequest, bool> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            signalX.Advanced.Trace($"Setting up  AuthenticationHandler ...");

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
            signalX.Advanced.Trace($"Setting up  OnConnectionEvent ...");
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
        public static void RespondToUser(this SignalX signalX, string user, string name, object data)
        {
            if (!signalX.AllowToSend(name, data))
                return;
            if (signalX.Settings.StartCountingOutGoingMessages)
                Interlocked.Increment(ref signalX.Settings.OutGoingCounter);

            signalX.Receiver.Receive(user, name, data);
        }

        /// <summary>
        ///     Reply to every other clients but the sender
        /// </summary>
        /// <param name="signalX"></param>
        /// <param name="excludedUserId"></param>
        /// <param name="name"></param>
        /// <param name="data"></param>
        /// <param name="groupName"></param>
        public static void RespondToOthers(this SignalX signalX, string excludedUserId, string name, object data, string groupName = null)
        {
            if (!signalX.AllowToSend(name, data))
                return;
            if (signalX.Settings.StartCountingOutGoingMessages)
                Interlocked.Increment(ref signalX.Settings.OutGoingCounter);

            signalX.Receiver.ReceiveAsOther(name, data, excludedUserId, groupName);
        }

        internal static async Task CallServer(this SignalX signalX, SignalXRequest request)
        {
            signalX.Advanced.Trace($"Running call to server {request?.Handler}...");
            ServerHandlerDetails executionDetails = signalX.SignalXServerExecutionDetails[request.Handler];
            if (executionDetails.IsSingleWriter)
                using (executionDetails.SingleWriter.Write())
                {
                   await  signalX.SignalXServers[request.Handler].Invoke(request, signalX.SignalXServerExecutionDetails[request.Handler].State).ConfigureAwait(false);
                }
            else
               await (signalX.SignalXServers[request.Handler]).Invoke(request, signalX.SignalXServerExecutionDetails[request.Handler].State).ConfigureAwait(false);
        }

        public static void RunJavaScriptOnAllClientsInGroup(this SignalX signalX, string script, string groupName, Action<dynamic> onResponse = null, TimeSpan? delay = null)
        {
            signalX.RunJavaScriptOnAllClientsInGroup(
                script,
                groupName,
                (a, b, c) => { onResponse?.Invoke(a); },
                delay);
        }

        public static void RunJavaScriptOnAllClients(this SignalX signalX, string script, Action<dynamic> onResponse = null, TimeSpan? delay = null)
        {
            signalX.RunJavaScriptOnAllClients(
                script,
                (a, b, c) => { onResponse?.Invoke(a); },
                delay);
        }

        public static void RunJavaScriptOnAllClients(this SignalX signalX, string script, Action<dynamic, SignalXRequest, string> onResponse = null, TimeSpan? delay = null)
        {
            signalX.RunJavaScriptOnAllClientsInGroup(script, null, onResponse, delay);
        }

        public static void RunJavaScriptOnAllClientsInGroup(this SignalX signalX, string script, string groupName, Action<dynamic, SignalXRequest, string> onResponse = null, TimeSpan? delay = null)
        {
            signalX.Advanced.Trace($"Running javascript on all clients in group {groupName} ...", script);

            if (signalX.OnResponseAfterScriptRuns != null)
            {
                //todo do something here to maybe block multiple calls
                //todo before a previous one finishes
                //todo or throw a warning
            }

            delay = delay ?? TimeSpan.FromSeconds(0);
            signalX.OnResponseAfterScriptRuns = onResponse;
            Task.Delay(delay.Value).ContinueWith(c => { signalX.RespondToAllInGroup(SignalX.SIGNALXCLIENTAGENT, script, groupName); });
        }

        /// <summary>
        /// Runs when client is ready before client's ready functions executes
        /// </summary>
        /// <param name="signalX"></param>
        /// <param name="onResponse"></param>
        public static void OnClientReady(this SignalX signalX, Action<SignalXRequest> onResponse)
        {
            signalX.Advanced.Trace($"Setting up  OnClientReady ...");
            signalX.OnClientReady.Add(onResponse);
        }
        /// <summary>
        /// Runs when client is ready before client's ready functions executes
        /// </summary>
        /// <param name="signalX"></param>
        /// <param name="onResponse"></param>
        public static void OnClientReady(this SignalX signalX, Action onResponse)
        {
            if (onResponse != null)
            {
                signalX.Advanced.Trace($"Setting up  OnClientReady ...");
                signalX.OnClientReady.Add((r) => { onResponse?.Invoke(); });
            }
        }

        public static void RunJavaScriptOnUser(this SignalX signalX, string userId, string script, Action<dynamic, SignalXRequest, string> onResponse = null, TimeSpan? delay = null)
        {
            signalX.Advanced.Trace($"Running javascript on user {userId}  ...", script);

            if (signalX.OnResponseAfterScriptRuns != null)
            {
                //todo do something here to maybe block multiple calls
                //todo before a previous one finishes
                //todo or throw a warning
            }

            delay = delay ?? TimeSpan.FromSeconds(0);
            signalX.OnResponseAfterScriptRuns = onResponse;
            Task.Delay(delay.Value).ContinueWith(c => { signalX.RespondToUser(userId, SignalX.SIGNALXCLIENTAGENT, script); });
        }

        public static void RunJavaScriptOnUser(this SignalX signalX, string userId, string script, Action<dynamic> onResponse = null, TimeSpan? delay = null)
        {
            signalX.RunJavaScriptOnUser(
                userId,
                script,
                (a, b, c) => { onResponse?.Invoke(a); },
                delay);
        }

        internal static void JoinGroup(
            this SignalX signalX,
            HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groups,
            string groupName)
        {
            signalX.Advanced.Trace($"User {context?.ConnectionId} is joining group {groupName}...");

            groups.Add(context?.ConnectionId, groupName);
            signalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.SignalXGroupJoin.ToString(), groupName));

            signalX.Receiver.ReceiveInGroupManager("join", context?.ConnectionId, groupName, context, clients, groups);
        }

        internal static void LeaveGroup(
            this SignalX signalX,
            HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groups,
            string groupName)
        {
            signalX.Advanced.Trace($"User {context?.ConnectionId} is leaving group {groupName}...");
            groups.Remove(context?.ConnectionId, groupName);
            signalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.SignalXGroupLeave.ToString(), groupName));
            signalX.Receiver.ReceiveInGroupManager("leave", context?.ConnectionId, groupName, context, clients, groups);
        }

        public static string GenerateUniqueNameId()
        {
            return "S" + Guid.NewGuid().ToString().Replace("-", "");
        }

        #endregion METHODS

        public static void RespondToScriptRequest(this SignalX signalX,
            HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groups)
        {
            signalX.Advanced.Trace($"Preparing script for client ...");
            signalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.SignalXRequestForMethods.ToString(), context?.User?.Identity?.Name));
            if (!signalX.CanProcess(context, "", null, true))
            {
                signalX.Settings.WarningHandler.ForEach(h => h?.Invoke("RequireAuthentication", "User attempting to connect has not been authenticated when authentication is required"));
                return;
            }

            var logRequestOnClient = signalX.Settings.LogAgentMessagesOnClient ? signalX.Settings.ClientLogScriptFunction + "(message);" : "";
            var logResponseOnClient = signalX.Settings.LogAgentMessagesOnClient ? signalX.Settings.ClientLogScriptFunction + "(response);" : "";
            var receiveErrorsFromClient = (signalX.Settings.ReceiveErrorMessagesFromClient
                ? signalX.ClientErrorSnippet : "") + (signalX.Settings.ReceiveDebugMessagesFromClient
                ? signalX.ClientDebugSnippet : "");
            var clientAgent = @"
                 " + receiveErrorsFromClient + @"
                 signalx.client." + SignalX.SIGNALXCLIENTAGENT + @" = function (message) {
                   " + logRequestOnClient + @"
                   var response ={};
                   try{ response={ Error : '', Result : eval('(function(){ '+message+' })()') }; }catch(err){  response = {Error : err, Result :'' }; signalx.error.f({error:err,description:'error occured while evaluating server agent command',context:'server agent error context'});  }
                   " + logResponseOnClient + @"
                    signalx.server." + SignalX.SIGNALXCLIENTAGENT + @"(response,function(messageResponse){  });
                 };";

            var clientReady = signalX.OnClientReady.Count == 0 ? "" : @"
                    signalx.beforeOthersReady=function(f){
                        signalx.debug.f('Checking with server before executing ready functions...')
                        signalx.server." + SignalX.SIGNALXCLIENTREADY + @"('',function(){
                             signalx.debug.f('Server indicates readiness. Now running client ready functions ... ');
                             typeof f ==='function' && f();
                          });
                 }";

            var methods = "; window.signalxidgen=window.signalxidgen||function(){return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {    var r = Math.random()*16|0, v = c == 'x' ? r : (r&0x3|0x8);    return v.toString(16);})};" +
                signalX.SignalXServers.Aggregate(clientReady + clientAgent + "var $sx= {", (current, signalXServer) => current + (signalXServer.Key + @":function(m,repTo,sen,msgId){ var deferred = $.Deferred();  window.signalxid=window.signalxid||window.signalxidgen();   sen=sen||window.signalxid;repTo=repTo||''; var messageId=window.signalxidgen(); var rt=repTo; if(typeof repTo==='function'){ signalx.waitingList(messageId,repTo);rt=messageId;  }  if(!repTo){ signalx.waitingList(messageId,deferred);rt=messageId;  }  var messageToSend={handler:'" + signalXServer.Key + "',message:m, replyTo:rt,sender:sen, groupList:signalx.groupList}; console.log(JSON.stringify(signalx.groupList));  chat.server.send('" + signalXServer.Key + "',m ||'',rt,sen,messageId,signalx.groupList||[]); console.log('sending ..'+JSON.stringify(signalx.groupList)+'  to '+'" + signalXServer.Key + "'); if(repTo){return messageId}else{ return deferred.promise();}   },")).Trim()
                + "}; $sx; ";

            if (signalX.Settings.StartCountingInComingMessages)
                Interlocked.Increment(ref signalX.Settings.InComingCounter);

            signalX.Advanced.Trace($"Sending script to client ...");
            signalX.Receiver.ReceiveScripts(context?.ConnectionId, methods, context, groups, clients);

            signalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.SignalXRequestForMethodsCompleted.ToString(), methods));
        }



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
            List<string> groupNames =null
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

            signalX.Advanced.Trace($"Creating a server {name} with authorized groups {string.Join(",", groupNames ?? new List<string>())} and requires authorization : {requireAuthorization}, is set to single write : {isSingleWriter} and allows dynamic server for this instance {allowDynamicServerForThisInstance}");

            groupNames = groupNames ?? new List<string>();
            name = name.Trim();
            string camelCased = char.ToLowerInvariant(name[0]) + name.Substring(1);
            string unCamelCased = char.ToUpperInvariant(name[0]) + name.Substring(1);

            if ((signalX.SignalXServers.ContainsKey(camelCased) || signalX.SignalXServers.ContainsKey(unCamelCased)) && !signalX.Settings.AllowDynamicServerInternal && !allowDynamicServerForThisInstance)
            {
                var exc = new Exception("Server with name '" + name + "' has already been created");
                signalX.Advanced.Trace(exc.Message, exc);

                throw exc;
            }

            try
            {
                if (signalX.SignalXServers.ContainsKey(camelCased))
                {
                    signalX.SignalXServers[camelCased] = server;
                    signalX.SignalXServerExecutionDetails[camelCased] = new ServerHandlerDetails(requireAuthorization, isSingleWriter, groupNames);

                    if (camelCased != unCamelCased)
                    {
                        signalX.SignalXServers[unCamelCased] = server;
                        signalX.SignalXServerExecutionDetails[unCamelCased] = new ServerHandlerDetails(requireAuthorization, isSingleWriter, groupNames);
                    }
                }
                else
                {
                    signalX.SignalXServers.GetOrAdd(camelCased, server);
                    signalX.SignalXServerExecutionDetails.GetOrAdd(camelCased, new ServerHandlerDetails(requireAuthorization, isSingleWriter, groupNames));

                    if (camelCased != unCamelCased)
                    {
                        signalX.SignalXServers.GetOrAdd(unCamelCased, server);
                        signalX.SignalXServerExecutionDetails.GetOrAdd(unCamelCased, new ServerHandlerDetails(requireAuthorization, isSingleWriter, groupNames));
                    }
                }
            }
            catch (Exception e)
            {
                signalX.Advanced.Trace($"Error creating a server {name} with authorized groups {string.Join(",", groupNames ?? new List<string>())} and requires authorization : {requireAuthorization}, is set to single write : {isSingleWriter} and allows dynamic server for this instance {allowDynamicServerForThisInstance}", e);

                signalX.Settings.ExceptionHandler.ForEach(h => h?.Invoke($"Error while creating server {name}", e));
            }
        }

        public static void ServerAsync(this SignalX signalX, string name, Func<SignalXRequest, SignalXServerState, Task> server, List<string> groupNames = null)
        {
            signalX.ServerBase(ServerType.Default, name,  server, groupNames: groupNames);
        }
        public static void ServerAsync(this SignalX signalX, string name, Func<SignalXRequest, Task> server, List<string> groupNames = null)
        {
            signalX.ServerBase(ServerType.Default, name, (r,s)=> server(r), groupNames: groupNames);
        }
        public static void ServerAsync(this SignalX signalX, ServerType serverType, string name, Func<SignalXRequest, SignalXServerState, Task> server, List<string> groupNames = null)
        {
            signalX.ServerBase(serverType, name,  server, groupNames: groupNames);
        }

        public static void ServerAsync(this SignalX signalX, ServerType serverType, string name, Func<SignalXRequest, Task> server, List<string> groupNames = null)
        {
            signalX.ServerBase(serverType, name, (r, s) => server(r), groupNames: groupNames);
        }
        public static void Server(this SignalX signalX, string name, Action<SignalXRequest, SignalXServerState> server, List<string> groupNames = null)
        {
            signalX.ServerBase(ServerType.Default, name, (r, s) => {
                server(r,s);
                return Task.FromResult(true);
            }, groupNames: groupNames);
        }
        public static void Server(this SignalX signalX, string name, Action<SignalXRequest> server, List<string> groupNames = null)
        {
            signalX.ServerBase(ServerType.Default, name, (r, s) => {
                server(r);
                return Task.FromResult(true);
            }, groupNames: groupNames);
        }
        public static void Server(this SignalX signalX, ServerType serverType, string name, Action<SignalXRequest, SignalXServerState> server, List<string> groupNames = null)
        {
            signalX.ServerBase(serverType, name, (r, s) => {
                server(r,s);
                return Task.FromResult(true);
            }, groupNames: groupNames);
        }
        public static void Server(this SignalX signalX, ServerType serverType, string name, Action<SignalXRequest> server, List<string> groupNames = null)
        {
            signalX.ServerBase(serverType, name, (r, s) => {
                server(r);
                return Task.FromResult(true);
            }, groupNames: groupNames);
        }

        
        #endregion
    }
}