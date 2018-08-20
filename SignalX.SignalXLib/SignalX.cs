using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Concurrent;

namespace SignalXLib.Lib
{
    using Microsoft.AspNet.SignalR.Hubs;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public partial class SignalX : IDisposable
    {
        public static ISignalXClientReceiver Receiver = new DefaultSignalRClientReceiver();

        internal static void UpdateClient(string clientMethodName, bool status)
        {
            if (string.IsNullOrEmpty(clientMethodName))
                throw new ArgumentNullException(nameof(clientMethodName));
            if (SignalXClientDetails.ContainsKey(clientMethodName))
            {
                SignalXClientDetails[clientMethodName].Disabled = status;
            }
            else
            {
                SignalXClientDetails.GetOrAdd(clientMethodName, new ClientDetails() { Disabled = status });
            }
        }

        /// <summary>
        /// Stops message sending to a given client
        /// </summary>
        public static void DisableClient(string clientMethodName)
        {
            UpdateClient(clientMethodName, true);
        }

        public static void DisableAllClients()
        {
            DisabledAllClients = true;
            foreach (string s in SignalXClientDetails.Select(x => x.Key).ToList())
            {
                UpdateClient(s, true);
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
        public static void SetGlobalDefaultMessageBufferSize(int defaultMessageBufferSize=1000)
        {
            GlobalHost.Configuration.DefaultMessageBufferSize = defaultMessageBufferSize;
        }
        public static void EnableClient(string clientMethodName)
        {
            UpdateClient(clientMethodName, false);
        }

        public static void EnableAllClients()
        {
            DisabledAllClients = false;
            foreach (string s in SignalXClientDetails.Select(x => x.Key).ToList())
            {
                UpdateClient(s, false);
            }
        }

        internal static bool DisabledAllClients { set; get; }

        public static HubConfiguration HubConfiguration = new HubConfiguration() { EnableDetailedErrors = true };
        internal static bool AllowDynamicServerInternal { set; get; }

        /// <summary>
        /// Powerful when this is true, you will be able to redefine a server by name at run time
        /// </summary>
        public static bool AllowDynamicServer
        {
            set { AllowDynamicServerInternal = value; }
        }

        public IDisposable MyApp { get; set; }
        internal static string AuthenticationCookieName = "";

        internal static bool CanProcess(HubCallerContext context, string serverHandlerName)
        {
            bool result = false;
            if (SignalX.RequireAuthorizationForAllHandlers || (SignalXServerExecutionDetails.ContainsKey(serverHandlerName) && SignalXServerExecutionDetails[serverHandlerName].RequiresAuthorization))
            {
                if (SignalX.AuthenticatedWhen != null)
                {
                    result = IsAuthenticated(context.Request);
                }
                else
                {
                    result = context.User.Identity.IsAuthenticated;
                    if (!result)
                        SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXRequestAuthorizationFailed.ToString(), $"Authorization failed after checking with context.User.Identity.IsAuthenticated. Custom Authorization check is not yet setup ");
                }
            }
            else
            {
                WarningHandler?.Invoke("AuthorizationNotSet", "Try setting an authorization handler to prevent anonymous access into your server");
                result = true;
            }
            return result;
        }

        internal static bool IsAuthenticated(IRequest request)
        {
            bool result;
            try
            {
                //var cookie = request?.Cookies[AuthenticationCookieName];
                //var ip = request?.Environment["server.RemoteIpAddress"]?.ToString();
                result = AuthenticatedWhen(request);
                if (!result)
                    ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXRequestAuthorizationFailed.ToString(), "Authorization failed after checking with Custom Authorization provided");
            }
            catch (Exception e)
            {
                result = false;
                ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXRequestAuthorizationFailed.ToString(), "Custom Authorization threw an exception " + e.Message);
                ExceptionHandler?.Invoke("Authentication failed", e);
            }

            return result;
        }

        internal static Func<IRequest, bool> AuthenticatedWhen { set; get; }

        /// <summary>
        /// Cookie authentication supported
        /// </summary>
        //public static void AuthenticationHandler(string cookieName, Func<Cookie,string, string, IRequest, bool> handler)
        //{
        //    AuthenticationCookieName = cookieName ?? throw new ArgumentNullException(nameof(cookieName));
        //    AuthenticatedWhen = handler ?? throw new ArgumentNullException(nameof(handler));
        //}
        public static void AuthenticationHandler(Func<IRequest, bool> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            AuthenticationCookieName = null;
            AuthenticatedWhen = handler;
        }

        internal static Action<string, object> WarningHandler { set; get; }

        public static void OnWarning(Action<string, object> handler)
        {
            WarningHandler = handler;
        }

        internal static Action<string, object> ConnectionEventsHandler { set; get; }

        public static ConnectionMapping<string> Connections = new ConnectionMapping<string>();

        public static bool ManageUserConnections { set; get; }
        internal static Action<string, Exception> ExceptionHandler { set; get; }

        public static void OnException(Action<Exception> handler)
        {
            ExceptionHandler = (m, e) => handler.Invoke(e);
        }

        public static void OnException(Action<string, Exception> handler)
        {
            ExceptionHandler = handler;
        }

        public static void OnConnectionEvent(Action<string, object> handler)
        {
            ConnectionEventsHandler = handler;
        }

        internal static bool HasHadAConnection { set; get; }

        public static bool RequireAuthorizationForAllHandlers { get; set; }
        public static bool RequireAuthorizationForPersistentConnections { get; set; }

        public void Dispose()
        {
            MyApp.Dispose();
        }

        internal static ConcurrentDictionary<string, ClientDetails> SignalXClientDetails = new ConcurrentDictionary<string, ClientDetails>();

        internal static ConcurrentDictionary<string, ServerHandlerDetails> SignalXServerExecutionDetails = new ConcurrentDictionary<string, ServerHandlerDetails>();

        protected internal static ConcurrentDictionary<string, Action<SignalXRequest, SignalXServerState>> SignalXServers = new ConcurrentDictionary<string, Action<SignalXRequest, SignalXServerState>>();

        public static void Server(string name,
            Action<SignalXRequest, SignalXServerState> server, 
            bool requireAuthorization = false, 
            bool isSingleWriter = false)
        {
            name = name.Trim();
            var camelCased = Char.ToLowerInvariant(name[0]) + name.Substring(1);
            var unCamelCased = Char.ToUpperInvariant(name[0]) + name.Substring(1);
            if ((SignalXServers.ContainsKey(camelCased) || SignalXServers.ContainsKey(unCamelCased)) && !AllowDynamicServerInternal)
            {
                throw new Exception("Server with name '" + name + "' already created");
            }

            try
            {
                if (SignalXServers.ContainsKey(camelCased))
                {
                    SignalXServers[camelCased] = server;
                    SignalXServerExecutionDetails[camelCased] = new ServerHandlerDetails(requireAuthorization, isSingleWriter);

                    if (camelCased != unCamelCased)
                    {
                        SignalXServers[unCamelCased] = server;
                        SignalXServerExecutionDetails[unCamelCased] = new ServerHandlerDetails(requireAuthorization, isSingleWriter);
                    }
                }
                else
                {
                    SignalXServers.GetOrAdd(camelCased, server);
                    SignalXServerExecutionDetails.GetOrAdd(camelCased, new ServerHandlerDetails(requireAuthorization, isSingleWriter));

                    if (camelCased != unCamelCased)
                    {
                        SignalXServers.GetOrAdd(unCamelCased, server);
                        SignalXServerExecutionDetails.GetOrAdd(unCamelCased, new ServerHandlerDetails(requireAuthorization, isSingleWriter));
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionHandler?.Invoke($"Error while creating server {name}", e);
            }
        }

        public static void ServerAuthorized(string name, Action<SignalXRequest> server)
        {
            ServerAuthorized(name, (r, s) => server(r));
        }
        public static void ServerAuthorizedSingleAccess(string name, Action<SignalXRequest> server)
        {
            ServerAuthorizedSingleAccess(name, (r, s) => server(r));
        }

        public static void Server(string name, Action<SignalXRequest> server, bool requireAuthorization = false)
        {
            Server(name, (r, s) => server(r), requireAuthorization);
        }

        public static void ServerSingleAccess(string name, Action<SignalXRequest> server, bool requireAuthorization = false)
        {
            Server(name, (r, s) => server(r), requireAuthorization,true);
        }
        public static void ServerAuthorized(string name, Action<SignalXRequest, SignalXServerState> server)
        {
            Server(name, server, true);
        }
        public static void ServerAuthorizedSingleAccess(string name, Action<SignalXRequest, SignalXServerState> server)
        {
            Server(name, server, true,true);
        }

        private static bool AllowToSend(string name, object data)
        {
            if (DisabledAllClients)
            {
                if (!SignalXClientDetails.ContainsKey(name) ||
                    (SignalXClientDetails.ContainsKey(name) && SignalXClientDetails[name].Disabled))
                {
                    SignalX.WarningHandler?.Invoke("DataSendingNotActivated",
                        new
                        {
                            Name = name,
                            Data = data,
                            Issue = $"Sending message to client {name} has been disabled. DisabledAllClients is activated"
                        });
                    return false;
                }
            }
            else
            {
                if (SignalXClientDetails.ContainsKey(name) && SignalXClientDetails[name].Disabled)
                {
                    SignalX.WarningHandler?.Invoke("DataSendingNotActivated",
                        new
                        {
                            Name = name,
                            Data = data,
                            Issue = $"Sending message to client {name} has been disabled"
                        });
                    return false;
                }
            }

            if (!SignalX.HasHadAConnection)
            {
                SignalX.WarningHandler?.Invoke("DataSendingNotActivated",
                    new
                    {
                        UserId = "",
                        Name = name,
                        Data = data,
                        Issue = "Cannot send message to client since no client connection has been obtained"
                    });
                return false;
            }
            return true;
        }

        internal static bool StartCountingOutGoingMessages { set; get; }
        internal static bool StartCountingInComingMessages { set; get; }

        public static async Task<double> GetOutGoingMessageSpeedAsync(TimeSpan duration)
        {
            OutGoingCounter = 0;
            var sw = new Stopwatch();
            StartCountingOutGoingMessages = true;
            sw.Start();
            await Task.Delay(duration);// might not be 5 sec
            StartCountingOutGoingMessages = false;
            sw.Stop();
            long currentCount = Interlocked.Read(ref OutGoingCounter);
            return currentCount / sw.Elapsed.TotalSeconds;
        }

        public static async Task<double> GetInComingMessageSpeedAsync(TimeSpan duration)
        {
            InComingCounter = 0;
            var sw = new Stopwatch();
            StartCountingInComingMessages = true;
            sw.Start();
            await Task.Delay(duration);// might not be 5 sec
            StartCountingInComingMessages = false;
            sw.Stop();
            long currentCount = Interlocked.Read(ref InComingCounter);
            return currentCount / sw.Elapsed.TotalSeconds;
        }

        internal static long OutGoingCounter;
        internal static long InComingCounter;

        public static void RespondToAll(string name, object data, string groupName = null)
        {
            if (!AllowToSend(name, data))
            {
                return;
            }
            if (StartCountingOutGoingMessages)
                Interlocked.Increment(ref OutGoingCounter);

            Receiver.Receive(name, data, groupName);
        }

        public static void RespondToUser(string userId, string name, object data)
        {
            if (!AllowToSend(name, data))
            {
                return;
            }
            if (StartCountingOutGoingMessages)
                Interlocked.Increment(ref OutGoingCounter);

            Receiver.Receive(userId, name, data);
        }

        public static void RespondToOthers(string excludedUserId, string name, object data, string groupName = null)
        {
            if (!AllowToSend(name, data))
            {
                return;
            }
            if (StartCountingOutGoingMessages)
                Interlocked.Increment(ref OutGoingCounter);

            Receiver.ReceiveAsOther(name, data, excludedUserId, groupName);
        }

        public static void SendMessageToServer(HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients, IGroupManager groups, string handler, object message, string replyTo, object sender, string messageId)
        {
            var user = context?.User;
            string error = "";
            try
            {
                if (SignalX.StartCountingInComingMessages)
                    Interlocked.Increment(ref SignalX.InComingCounter);

                SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXIncomingRequest.ToString(), new
                {
                    handler = handler,
                    message = message,
                    replyTo = replyTo,
                    sender = sender,
                    messageId = messageId
                });

                if (string.IsNullOrEmpty(handler) || !SignalXServers.ContainsKey(handler))
                {
                    var e = "Error request for unknown server name " + handler;
                    SignalX.ExceptionHandler?.Invoke(e, new Exception(e));
                    SignalX.RespondToUser(context?.ConnectionId, replyTo, e);
                    return;
                }

                if (!SignalX.CanProcess(context, handler))
                {
                    SignalX.WarningHandler?.Invoke("RequireAuthentication", "User attempting to connect has not been authenticated when authentication is required");
                    return;
                }

                var request = new SignalXRequest(replyTo, sender, messageId, message, context?.User?.Identity?.Name, context?.ConnectionId, handler, context?.User);

                CallServer(request);
            }
            catch (Exception e)
            {
                error = "An error occured on the server while processing message " + message + " with id " +
                           messageId + " received from  " + sender + " [ user = " + user.Identity.Name + "] for a response to " + replyTo + " - ERROR: " +
                           e.Message;
                SignalX.RespondToUser(context.ConnectionId, "signalx_error", error);
                if (!string.IsNullOrEmpty(replyTo))
                {
                    SignalX.RespondToUser(context.ConnectionId, replyTo, error);
                }
                SignalX.ExceptionHandler?.Invoke(error, e);
            }
            SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXIncomingRequestCompleted.ToString(), new
            {
                handler = handler,
                message = message,
                replyTo = replyTo,
                sender = sender,
                messageId = messageId,
                error = error
            });
        }

        private static void CallServer(SignalXRequest request)
        {
            var executionDetails = SignalXServerExecutionDetails[request.Handler];
            if (executionDetails.IsSingleWriter)
            {
                using (executionDetails.SingleWriter.Write())
                {
                    SignalX.SignalXServers[request.Handler].Invoke(request, SignalXServerExecutionDetails[request.Handler].State);
                }
            }
            else
            {
                SignalX.SignalXServers[request.Handler].Invoke(request, SignalXServerExecutionDetails[request.Handler].State);
            }
        }

        public static void RespondToScriptRequest(
            HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groups)
        {
            SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXRequestForMethods.ToString(), context?.User?.Identity?.Name);
            if (!SignalX.CanProcess(context, ""))
            {
                SignalX.WarningHandler?.Invoke("RequireAuthentication", "User attempting to connect has not been authenticated when authentication is required");
                return;
            }
            var methods = SignalX.SignalXServers.Aggregate("window.signalxidgen=window.signalxidgen||function(){return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {    var r = Math.random()*16|0, v = c == 'x' ? r : (r&0x3|0x8);    return v.toString(16);})};var $sx= {", (current, signalXServer) => current + (signalXServer.Key + @":function(m,repTo,sen,msgId){ var deferred = $.Deferred();  window.signalxid=window.signalxid||window.signalxidgen();   sen=sen||window.signalxid;repTo=repTo||''; var messageId=window.signalxidgen(); var rt=repTo; if(typeof repTo==='function'){ signalx.waitingList(messageId,repTo);rt=messageId;  }  if(!repTo){ signalx.waitingList(messageId,deferred);rt=messageId;  }  chat.server.send('" + signalXServer.Key + "',m ||'',rt,sen,messageId); if(repTo){return messageId}else{ return deferred.promise();}   },")) + "}; $sx; ";

            if (SignalX.StartCountingInComingMessages)
                Interlocked.Increment(ref SignalX.InComingCounter);

            Receiver.ReceiveScripts(context?.ConnectionId, methods, context, groups, clients);

            SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXRequestForMethodsCompleted.ToString(), methods);
        }

        internal static void JoinGroup(HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groups, string groupName)
        {
            groups.Add(context?.ConnectionId, groupName);
            SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXGroupJoin.ToString(), groupName);

            Receiver.ReceiveInGroupManager(context?.ConnectionId, groupName, context, clients, groups);
        }

        internal static void LeaveGroup(HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groups, string groupName)
        {
            groups.Remove(context?.ConnectionId, groupName);
            SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXGroupLeave.ToString(), groupName);

            Receiver.ReceiveInGroupManager(context?.ConnectionId, groupName, context, clients, groups);
        }
    }
}