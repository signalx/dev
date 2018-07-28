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
        private static ISignalXClientReceiver Receiver;

        public SignalX(HubConfiguration hubConfiguration = null, ISignalXClientReceiver receiver = null
        )
        {
            Receiver = receiver ?? new DefaultSignalRClientReceiver();
            Connections = new ConnectionMapping<string>();
            HubConfiguration = hubConfiguration ?? new HubConfiguration();
        }

        internal static void UpdateClient(string clientMethodName, bool status)
        {
            if (string.IsNullOrEmpty(clientMethodName))
                throw new ArgumentNullException(nameof(clientMethodName));
            if (_signalXClientDetails.ContainsKey(clientMethodName))
            {
                _signalXClientDetails[clientMethodName].Disabled = status;
            }
            else
            {
                _signalXClientDetails.GetOrAdd(clientMethodName, new ClientDetails() { Disabled = status });
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
            foreach (string s in _signalXClientDetails.Select(x => x.Key).ToList())
            {
                UpdateClient(s, true);
            }
        }

        public static void EnableClient(string clientMethodName)
        {
            UpdateClient(clientMethodName, false);
        }

        public static void EnableAllClients()
        {
            DisabledAllClients = false;
            foreach (string s in _signalXClientDetails.Select(x => x.Key).ToList())
            {
                UpdateClient(s, false);
            }
        }

        internal static bool DisabledAllClients { set; get; }

        public HubConfiguration HubConfiguration { get; set; }
        internal static bool AllowDynamicServerInternal { set; get; }

        /// <summary>
        /// Powerful when this is true, you will be able to redefine a server by name at run time
        /// </summary>
        public static bool AllowDynamicServer
        {
            set { AllowDynamicServer = value; }
        }

        public IDisposable MyApp { get; set; }
        internal static string AuthenticationCookieName = "";

        internal static bool CanProcess(HubCallerContext context, string serverHandlerName)
        {
            bool result = false;
            if (SignalX.RequireAuthorizationForAllHandlers || (_signalXServersDetails.ContainsKey(serverHandlerName) && _signalXServersDetails[serverHandlerName].RequiresAuthorization))
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

        internal static ConcurrentDictionary<string, ClientDetails> _signalXClientDetails = new ConcurrentDictionary<string, ClientDetails>();

        internal static ConcurrentDictionary<string, ServerHandlerDetails> _signalXServersDetails = new ConcurrentDictionary<string, ServerHandlerDetails>();

        internal static ConcurrentDictionary<string, Action<object, object, string, string, string, string>> _signalXServers = new ConcurrentDictionary<string, Action<object, object, string, string, string, string>>();

        public static void Server(string name, Action<object, object, string, string, string, string> server, bool requireAuthorization = false)
        {
            name = name.Trim();
            if (_signalXServers.ContainsKey(name) && !AllowDynamicServerInternal)
            {
                throw new Exception("Server with name '" + name + "' already created");
            }

            try
            {
                var camelCased = Char.ToLowerInvariant(name[0]) + name.Substring(1);

                var unCamelCased = Char.ToUpperInvariant(name[0]) + name.Substring(1);

                if (_signalXServers.ContainsKey(camelCased))
                {
                    _signalXServers[camelCased] = server;
                    if (!requireAuthorization)
                    {
                        _signalXServersDetails[camelCased] = new ServerHandlerDetails(false);
                    }
                    else
                    {
                        _signalXServersDetails[camelCased] = new ServerHandlerDetails(true);
                    }
                    if (camelCased != unCamelCased)
                    {
                        _signalXServers[unCamelCased] = server;
                        if (!requireAuthorization)
                        {
                            _signalXServersDetails[unCamelCased] = new ServerHandlerDetails(false);
                        }
                        else
                        {
                            _signalXServersDetails[unCamelCased] = new ServerHandlerDetails(true);
                        }
                    }
                }
                else
                {
                    _signalXServers.GetOrAdd(camelCased, server);
                    if (!requireAuthorization)
                    {
                        _signalXServersDetails.GetOrAdd(camelCased, new ServerHandlerDetails(false));
                    }
                    else
                    {
                        _signalXServersDetails.GetOrAdd(camelCased, new ServerHandlerDetails(true));
                    }

                    if (camelCased != unCamelCased)
                    {
                        _signalXServers.GetOrAdd(unCamelCased, server);
                        if (!requireAuthorization)
                        {
                            _signalXServersDetails.GetOrAdd(unCamelCased, new ServerHandlerDetails(false));
                        }
                        else
                        {
                            _signalXServersDetails.GetOrAdd(unCamelCased, new ServerHandlerDetails(true));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionHandler?.Invoke($"Error while creating server {name}", e);
            }
        }

        public static void ServerAuthorized(string name, Action<object, object, string, string, string, string> server)
        {
            Server(name, server, true);
        }

        public static void ServerAuthorized(string name, Action<object, object, string, string, string> server)
        {
            ServerAuthorized(name, (message, sender, replyTo, messageId, userId, connectionId) => server(message, sender, replyTo, userId, connectionId));
        }

        public static void ServerAuthorized(string name, Action<object, object, string, string> server)
        {
            ServerAuthorized(name, (message, sender, replyTo, messageId, userId, connectionId) => server(message, sender, replyTo, userId));
        }

        public static void ServerAuthorized(string name, Action<object, object, string> server)
        {
            ServerAuthorized(name, (message, sender, replyTo, messageId, userId, connectionId) => server(message, sender, replyTo));
        }

        public static void ServerAuthorized(string name, Action<object, object> server)
        {
            ServerAuthorized(name, (message, sender, replyTo, messageId, userId, connectionId) => server(message, sender));
        }

        public static void ServerAuthorized(string name, Action<SignalXRequest> server)
        {
            ServerAuthorized(name, (message, sender, replyTo, messageId, userId, connectionId) => server(new SignalXRequest(replyTo, sender, messageId, message, userId, connectionId)));
        }

        public static void Server(string name, Action<object, object, string, string, string> server)
        {
            Server(name, (message, sender, replyTo, messageId, userId, connectionId) => server(message, sender, replyTo, userId, connectionId));
        }

        public static void Server(string name, Action<object, object, string, string> server)
        {
            Server(name, (message, sender, replyTo, messageId, userId, connectionId) => server(message, sender, replyTo, userId));
        }

        public static void Server(string name, Action<object, object, string> server)
        {
            Server(name, (message, sender, replyTo, messageId, userId, connectionId) => server(message, sender, replyTo));
        }

        public static void Server(string name, Action<object, object> server)
        {
            Server(name, (message, sender, replyTo, messageId, userId, connectionId) => server(message, sender));
        }

        public static void Server(string name, Action<SignalXRequest> server)
        {
            Server(name, (message, sender, replyTo, messageId, userId, connectionId) => server(new SignalXRequest(replyTo, sender, messageId, message, userId, connectionId)));
        }

        private static bool AllowToSend(string name, object data)
        {
            if (DisabledAllClients)
            {
                if (!_signalXClientDetails.ContainsKey(name) ||
                    (_signalXClientDetails.ContainsKey(name) && _signalXClientDetails[name].Disabled))
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
                if (_signalXClientDetails.ContainsKey(name) && _signalXClientDetails[name].Disabled)
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

        public static void RespondToAll(string name, object data)
        {
            if (!AllowToSend(name, data))
            {
                return;
            }
            if (StartCountingOutGoingMessages)
                Interlocked.Increment(ref OutGoingCounter);

            Receiver.Receive(name, data);
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
        public static void RespondToOthers(string excludedUserId, string name, object data)
        {
            if (!AllowToSend(name, data))
            {
                return;
            }
            if (StartCountingOutGoingMessages)
                Interlocked.Increment(ref OutGoingCounter);

            Receiver.ReceiveAsOther(name, data, excludedUserId);
        }
        internal static void Send(HubCallerContext context, IHubCallerConnectionContext<dynamic> clients, string handler, object message, string replyTo, object sender, string messageId)
        {
            var user = context?.User;
            string error = "";
            try
            {
                SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXIncomingRequest.ToString(), new
                {
                    handler = handler,
                    message = message,
                    replyTo = replyTo,
                    sender = sender,
                    messageId = messageId
                });
                if (!SignalX.CanProcess(context, handler))
                {
                    SignalX.WarningHandler?.Invoke("RequireAuthentication", "User attempting to connect has not been authenticated when authentication is required");
                    return;
                }

                if (SignalX.StartCountingInComingMessages)
                    Interlocked.Increment(ref SignalX.InComingCounter);

                SignalX._signalXServers[handler].Invoke(message, sender, replyTo, messageId, context?.User?.Identity?.Name, context?.ConnectionId);
            }
            catch (Exception e)
            {
                error = "An error occured on the server while processing message " + message + " with id " +
                           messageId + " received from  " + sender + " [ user = " + user.Identity.Name + "] for a response to " + replyTo + " - ERROR: " +
                           e.Message;
                SignalX.RespondToAll("signalx_error", error);
                if (!string.IsNullOrEmpty(replyTo))
                {
                    SignalX.RespondToAll(replyTo, error);
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
        internal static void GetMethods(HubCallerContext context, IHubCallerConnectionContext<dynamic> clients)
        {
            SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXRequestForMethods.ToString(), context?.User?.Identity?.Name);
            if (!SignalX.CanProcess(context, ""))
            {
                SignalX.WarningHandler?.Invoke("RequireAuthentication", "User attempting to connect has not been authenticated when authentication is required");
                return;
            }
            var methods = SignalX._signalXServers.Aggregate("var $sx= {", (current, signalXServer) => current + (signalXServer.Key + @":function(m,repTo,sen,msgId){ var deferred = $.Deferred(); window.signalxidgen=window.signalxidgen||function(){return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {    var r = Math.random()*16|0, v = c == 'x' ? r : (r&0x3|0x8);    return v.toString(16);})}; window.signalxid=window.signalxid||window.signalxidgen();   sen=sen||window.signalxid;repTo=repTo||''; var messageId=window.signalxidgen(); var rt=repTo; if(typeof repTo==='function'){ signalx.waitingList(messageId,repTo);rt=messageId;  }  if(!repTo){ signalx.waitingList(messageId,deferred);rt=messageId;  }  chat.server.send('" + signalXServer.Key + "',m,rt,sen,messageId); if(repTo){return messageId}else{ return deferred.promise();}   },")) + "}; $sx; ";

            if (SignalX.StartCountingInComingMessages)
                Interlocked.Increment(ref SignalX.InComingCounter);

            Receiver.ReceiveScripts(methods, clients);
            
            SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXRequestForMethodsCompleted.ToString(), methods);
        }
    }
}