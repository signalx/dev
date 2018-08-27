﻿namespace SignalXLib.Lib
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;

    public static class SignalXExtensions
    {
       
        #region METHODS
        internal static void UpdateClient(this SignalX signalX,   string clientMethodName, bool status)
        {
            if (string.IsNullOrEmpty(clientMethodName))
                throw new ArgumentNullException(nameof(clientMethodName));
            if (signalX.Settings.SignalXClientDetails.ContainsKey(clientMethodName))
            {
                signalX.Settings.SignalXClientDetails[clientMethodName].Disabled = status;
            }
            else
            {
                signalX.Settings.SignalXClientDetails.GetOrAdd(clientMethodName, new ClientDetails() { Disabled = status });
            }
        }

        /// <summary>
        /// Stops message sending to a given client
        /// </summary>
        public static void DisableClient(this SignalX signalX, string clientMethodName)
        {
            UpdateClient(signalX,clientMethodName, true);
        }

        public static void DisableAllClients(this SignalX signalX)
        {
            signalX.Settings.DisabledAllClients = true;
            foreach (string s in signalX.Settings.SignalXClientDetails.Select(x => x.Key).ToList())
            {
                signalX.UpdateClient(s, true);
            }
        }
        /// <summary>
        /// This just turns around to set the GlobalHost.Configuration.DefaultMessageBufferSize
        /// Since messages are stored in the message bus in server memory, reducing the size of messages can also address server memory issues.
        /// Since SignalR doesn't block when you call client methods, you can invoke client methods very quickly. 
        /// Unfortunately, the client might not always be ready to receive messages immediately once you send them, so SignalR has to buffer messages
        /// So by default SignalR will buffer up to 1000 messages per client. 
        /// Once the client falls behind by over 1000 messages, it will start missing messages
        /// By default, SignalR retains 1000 messages in memory per hub per connection. 
        /// If large messages are being used, this may create memory issues which can be 
        /// alleviated by reducing this value. This setting can be set in the 
        /// Application_Start event handler in an ASP.NET application, or in the 
        /// Configuration method of an OWIN startup class in a self-hosted application
        /// see https://docs.microsoft.com/en-us/aspnet/signalr/overview/performance/signalr-performance#tuning
        ///  </summary>
        /// <param name="defaultMessageBufferSize">Default val is 1000 </param>
        public static void SetGlobalDefaultMessageBufferSize(this SignalX signalX, int defaultMessageBufferSize = 1000)
        {
            GlobalHost.Configuration.DefaultMessageBufferSize = defaultMessageBufferSize;
        }
        public static void EnableClient(this SignalX signalX, string clientMethodName)
        {
            signalX.UpdateClient(clientMethodName, false);
        }

        public static void EnableAllClients(this SignalX signalX)
        {
            signalX.Settings.DisabledAllClients = false;
            foreach (string s in signalX.Settings.SignalXClientDetails.Select(x => x.Key).ToList())
            {
                signalX.UpdateClient(s, false);
            }
        }

        internal static bool CanProcess(this SignalX signalX, HubCallerContext context, string serverHandlerName)
        {
            bool result = false;
            if (signalX.Settings.RequireAuthorizationForAllHandlers || (signalX.Settings.SignalXServerExecutionDetails.ContainsKey(serverHandlerName) && signalX.Settings.SignalXServerExecutionDetails[serverHandlerName].RequiresAuthorization))
            {
                if (signalX.Settings.AuthenticatedWhen != null)
                {
                    result = signalX.IsAuthenticated(context.Request);
                }
                else
                {
                    result = context.User.Identity.IsAuthenticated;
                    if (!result)
                        signalX.Settings.ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXRequestAuthorizationFailed.ToString(), $"Authorization failed after checking with context.User.Identity.IsAuthenticated. Custom Authorization check is not yet setup ");
                }
            }
            else
            {
                signalX.Settings.WarningHandler?.Invoke("AuthorizationNotSet", "Try setting an authorization handler to prevent anonymous access into your server");
                result = true;
            }
            return result;
        }

        internal static bool IsAuthenticated(this SignalX signalX, IRequest request)
        {
            bool result;
            try
            {
                //var cookie = request?.Cookies[AuthenticationCookieName];
                //var ip = request?.Environment["server.RemoteIpAddress"]?.ToString();
                result = signalX.Settings.AuthenticatedWhen(request);
                if (!result)
                    signalX.Settings.ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXRequestAuthorizationFailed.ToString(), "Authorization failed after checking with Custom Authorization provided");
            }
            catch (Exception e)
            {
                result = false;
                signalX.Settings.ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXRequestAuthorizationFailed.ToString(), "Custom Authorization threw an exception " + e.Message);
                signalX.Settings.ExceptionHandler?.Invoke("Authentication failed", e);
            }

            return result;
        }



        /// <summary>
        /// Cookie authentication supported
        /// </summary>
        //public static void AuthenticationHandler(string cookieName, Func<Cookie,string, string, IRequest, bool> handler)
        //{
        //    AuthenticationCookieName = cookieName ?? throw new ArgumentNullException(nameof(cookieName));
        //    AuthenticatedWhen = handler ?? throw new ArgumentNullException(nameof(handler));
        //}
        public static void AuthenticationHandler(this SignalX signalX, Func<IRequest, bool> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            signalX.Settings.AuthenticationCookieName = null;
            signalX.Settings.AuthenticatedWhen = handler;
        }

        public static void OnWarning(this SignalX signalX, Action<string, object> handler)
        {
            signalX.Settings.WarningHandler = handler;
        }


        public static void OnException(this SignalX signalX, Action<Exception> handler)
        {
            signalX.Settings.ExceptionHandler = (m, e) => handler.Invoke(e);
        }

        public static void OnException(this SignalX signalX, Action<string, Exception> handler)
        {
            signalX.Settings.ExceptionHandler = handler;
        }

        public static void OnConnectionEvent(this SignalX signalX, Action<string, object> handler)
        {
            signalX.Settings.ConnectionEventsHandler = handler;
        }






        public static void Server(this SignalX signalX, string name,
            Action<SignalXRequest, SignalXServerState> server,
            bool requireAuthorization = false,
            bool isSingleWriter = false,
            bool allowDynamicServerForThisInstance = false)
        {
            name = name.Trim();
            var camelCased = Char.ToLowerInvariant(name[0]) + name.Substring(1);
            var unCamelCased = Char.ToUpperInvariant(name[0]) + name.Substring(1);

            if ((signalX.Settings.SignalXServers.ContainsKey(camelCased) || signalX.Settings.SignalXServers.ContainsKey(unCamelCased)) && !signalX.Settings.AllowDynamicServerInternal && !allowDynamicServerForThisInstance)
            {
                throw new Exception("Server with name '" + name + "' has already been created");
            }

            try
            {
                if (signalX.Settings.SignalXServers.ContainsKey(camelCased))
                {
                    signalX.Settings.SignalXServers[camelCased] = server;
                    signalX.Settings.SignalXServerExecutionDetails[camelCased] = new ServerHandlerDetails(requireAuthorization, isSingleWriter);

                    if (camelCased != unCamelCased)
                    {
                        signalX.Settings.SignalXServers[unCamelCased] = server;
                        signalX.Settings.SignalXServerExecutionDetails[unCamelCased] = new ServerHandlerDetails(requireAuthorization, isSingleWriter);
                    }
                }
                else
                {
                    signalX.Settings.SignalXServers.GetOrAdd(camelCased, server);
                    signalX.Settings.SignalXServerExecutionDetails.GetOrAdd(camelCased, new ServerHandlerDetails(requireAuthorization, isSingleWriter));

                    if (camelCased != unCamelCased)
                    {
                        signalX.Settings.SignalXServers.GetOrAdd(unCamelCased, server);
                        signalX.Settings.SignalXServerExecutionDetails.GetOrAdd(unCamelCased, new ServerHandlerDetails(requireAuthorization, isSingleWriter));
                    }
                }
            }
            catch (Exception e)
            {
                signalX.Settings.ExceptionHandler?.Invoke($"Error while creating server {name}", e);
            }
        }

        public static void ServerAuthorized(this SignalX signalX, string name, Action<SignalXRequest> server)
        {
            signalX.ServerAuthorized(name, (r, s) => server(r));
        }
        public static void ServerAuthorizedSingleAccess(this SignalX signalX, string name, Action<SignalXRequest> server)
        {
            signalX.ServerAuthorizedSingleAccess(name, (r, s) => server(r));
        }

        public static void Server(this SignalX signalX, string name, Action<SignalXRequest> server, bool requireAuthorization = false)
        {
            signalX.Server(name, (r, s) => server(r), requireAuthorization);
        }

        public static void ServerSingleAccess(this SignalX signalX, string name, Action<SignalXRequest> server, bool requireAuthorization = false)
        {
            signalX.Server(name, (r, s) => server(r), requireAuthorization, true);
        }
        public static void ServerAuthorized(this SignalX signalX, string name, Action<SignalXRequest, SignalXServerState> server)
        {
            signalX.Server(name, server, true);
        }
        public static void ServerAuthorizedSingleAccess(this SignalX signalX, string name, Action<SignalXRequest, SignalXServerState> server)
        {
            signalX.Server(name, server, true, true);
        }

       
        public static async Task<double> GetOutGoingMessageSpeedAsync(this SignalX signalX, TimeSpan duration)
        {
            signalX.Settings.OutGoingCounter = 0;
            var sw = new Stopwatch();
            signalX.Settings.StartCountingOutGoingMessages = true;
            sw.Start();
            await Task.Delay(duration);// might not be 5 sec
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
            await Task.Delay(duration);// might not be 5 sec
            signalX.Settings.StartCountingInComingMessages = false;
            sw.Stop();
            long currentCount = Interlocked.Read(ref signalX.Settings.InComingCounter);
            return currentCount / sw.Elapsed.TotalSeconds;
        }



        /// <summary>
        /// Reply to a specific client
        /// </summary>
        /// <param name="signalX"></param>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <param name="data"></param>
        public static void RespondToUser(this SignalX signalX, string userId, string name, object data)
        {
            if (!signalX.AllowToSend(name, data))
            {
                return;
            }
            if (signalX.Settings.StartCountingOutGoingMessages)
                Interlocked.Increment(ref signalX.Settings.OutGoingCounter);

            signalX.Settings.Receiver.Receive(userId, name, data);
        }

        /// <summary>
        ///  Reply to every other clients but the sender
        /// </summary>
        /// <param name="signalX"></param>
        /// <param name="excludedUserId"></param>
        /// <param name="name"></param>
        /// <param name="data"></param>
        /// <param name="groupName"></param>
        public static void RespondToOthers(this SignalX signalX, string excludedUserId, string name, object data, string groupName = null)
        {
            if (!signalX.AllowToSend(name, data))
            {
                return;
            }
            if (signalX.Settings.StartCountingOutGoingMessages)
                Interlocked.Increment(ref signalX.Settings.OutGoingCounter);

            signalX.Settings.Receiver.ReceiveAsOther(name, data, excludedUserId, groupName);
        }

      
        internal static void CallServer(this SignalX signalX, SignalXRequest request)
        {
            var executionDetails = signalX.Settings.SignalXServerExecutionDetails[request.Handler];
            if (executionDetails.IsSingleWriter)
            {
                using (executionDetails.SingleWriter.Write())
                {
                    signalX.Settings.SignalXServers[request.Handler].Invoke(request, signalX. Settings.SignalXServerExecutionDetails[request.Handler].State);
                }
            }
            else
            {
                signalX.Settings.SignalXServers[request.Handler].Invoke(request, signalX. Settings.SignalXServerExecutionDetails[request.Handler].State);
            }
        }


        public static void RunJavaScriptOnAllClients(this SignalX signalX, string script, Action<dynamic> onResponse = null, TimeSpan? delay = null)
        {
            signalX.RunJavaScriptOnAllClients( script,
                (a, b, c) =>
                {
                    onResponse?.Invoke(a);
                });
        }

        public static void RunJavaScriptOnAllClients(this SignalX signalX, string script, Action<dynamic, SignalXRequest, string> onResponse=null, TimeSpan? delay = null)
        {
            if (signalX.Settings.OnResponseAfterScriptRuns != null)
            {
                //todo do something here to maybe block multiple calls 
                //todo before a previous one finishes
                //todo or throw a warning
            }
            delay = delay ?? TimeSpan.FromSeconds(0);
            signalX.Settings.OnResponseAfterScriptRuns = onResponse;
            Task.Delay(delay.Value).ContinueWith((c) =>
            {
                signalX.RespondToAll(SignalX. SIGNALXCLIENTAGENT, script);
            });
        }

        public static void RespondToScriptRequest(this SignalX signalX,
            HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groups)
        {
            signalX.Settings.ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXRequestForMethods.ToString(), context?.User?.Identity?.Name);
            if (!signalX.CanProcess(context, ""))
            {
                signalX.Settings.WarningHandler?.Invoke("RequireAuthentication", "User attempting to connect has not been authenticated when authentication is required");
                return;
            }

            var logRequestOnClient = signalX.Settings.LogAgentMessagesOnClient ? "console.log(message);" : "";
            var logResponseOnClient = signalX.Settings.LogAgentMessagesOnClient ? "console.log(response);" : "";
            var clientAgent = @"
                 signalx.client." + SignalX. SIGNALXCLIENTAGENT + @" = function (message) {
                   " + logRequestOnClient + @"
                   var response ={};
                   try{ response={ Error : '', Result : eval('(function(){ '+message+' })()') }; }catch(err){  response = {Error : err, Result :'' }   }
                   " + logResponseOnClient + @"
                    signalx.server." + SignalX.SIGNALXCLIENTAGENT + @"(response,function(messageResponse){  });
                 };";
            var methods = signalX.Settings.SignalXServers.Aggregate(clientAgent + "window.signalxidgen=window.signalxidgen||function(){return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {    var r = Math.random()*16|0, v = c == 'x' ? r : (r&0x3|0x8);    return v.toString(16);})};var $sx= {", (current, signalXServer) => current + (signalXServer.Key + @":function(m,repTo,sen,msgId){ var deferred = $.Deferred();  window.signalxid=window.signalxid||window.signalxidgen();   sen=sen||window.signalxid;repTo=repTo||''; var messageId=window.signalxidgen(); var rt=repTo; if(typeof repTo==='function'){ signalx.waitingList(messageId,repTo);rt=messageId;  }  if(!repTo){ signalx.waitingList(messageId,deferred);rt=messageId;  }  chat.server.send('" + signalXServer.Key + "',m ||'',rt,sen,messageId); if(repTo){return messageId}else{ return deferred.promise();}   },")) + "}; $sx; ";

            if (signalX.Settings.StartCountingInComingMessages)
                Interlocked.Increment(ref signalX.Settings.InComingCounter);

            signalX.Settings.Receiver.ReceiveScripts(context?.ConnectionId, methods, context, groups, clients);

            signalX.Settings.ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXRequestForMethodsCompleted.ToString(), methods);
        }

        internal static void JoinGroup(this SignalX signalX, HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groups, string groupName)
        {
            groups.Add(context?.ConnectionId, groupName);
            signalX.Settings.ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXGroupJoin.ToString(), groupName);

            signalX.Settings.Receiver.ReceiveInGroupManager(context?.ConnectionId, groupName, context, clients, groups);
        }

        internal static void LeaveGroup(this SignalX signalX, HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groups, string groupName)
        {
            groups.Remove(context?.ConnectionId, groupName);
            signalX.Settings.ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXGroupLeave.ToString(), groupName);

            signalX.Settings.Receiver.ReceiveInGroupManager(context?.ConnectionId, groupName, context, clients, groups);
        }

        #endregion
    }
}