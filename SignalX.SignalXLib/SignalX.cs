using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using System;
using System.Collections.Concurrent;

namespace SignalXLib.Lib
{
    using Microsoft.AspNet.SignalR.Hubs;

    public class SignalX : IDisposable
    {
        public SignalX(HubConfiguration hubConfiguration = null
        )
        {
            if (hubConfiguration == null) throw new ArgumentNullException(nameof(hubConfiguration));
            HubConfiguration = hubConfiguration;
        }

        public HubConfiguration HubConfiguration { get; set; }
        internal static bool AllowDynamicServerInternal { set; get; }

        /// <summary>
        /// Powerful when this is true, you will be able to redefine a server by name at run time
        /// </summary>
        public bool AllowDynamicServer
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
                WarningHandler?.Invoke("AuthorizationNotSet","Try setting an authorization handler to prevent anonymous access into your server");
                result = true;
            }
            return result;
        }

       internal static bool IsAuthenticated(IRequest request)
        {
            bool result;
            try
            {
                var cookie = GetCookieValue(request, AuthenticationCookieName);
                var ip = request.Environment["server.RemoteIpAddress"].ToString();
                result = AuthenticatedWhen(cookie, ip, request);
                if (!result)
                    ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXRequestAuthorizationFailed.ToString(), "Authorization failed after checking with Custom Authorization provided");
            }
            catch (Exception e)
            {
                result = false;
                ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXRequestAuthorizationFailed.ToString(), "Custom Authorization threw an exception " + e.Message);
                ExceptionHandler?.Invoke(e);
            }

            return result;
        }
        
        internal static Cookie GetCookieValue(IRequest request, string cookieName)
        {
            var cookie = !string.IsNullOrEmpty(cookieName) &&
                request != null &&
                request.Cookies != null &&
                request.Cookies.ContainsKey(cookieName) ? request?.Cookies[cookieName] : null;
            return cookie;
        }

        internal static Func<Cookie, string, IRequest, bool> AuthenticatedWhen { set; get; }

        /// <summary>
        /// Cookie authentication supported
        /// </summary>
        public static void AuthenticationHandler(string cookieName, Func<Cookie, string, IRequest, bool> handler)
        {
            AuthenticationCookieName = cookieName ?? throw new ArgumentNullException(nameof(cookieName));
            AuthenticatedWhen = handler ?? throw new ArgumentNullException(nameof(handler));
        }
        public static void AuthenticationHandler(Func<IRequest, bool> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            AuthenticationCookieName = null;
            AuthenticatedWhen =  (a, b, r) => handler(r);
        }
        internal static Action<string, object> WarningHandler { set; get; }

        public static void OnWarning(Action<string, object> handler)
        {
            WarningHandler = handler;
        }

        internal static Action<string, object> ConnectionEventsHandler { set; get; }

        public static ConnectionMapping<string> Connections { set; get; }

        public static bool ManageUserConnections { set; get; }
        internal static Action<Exception> ExceptionHandler { set; get; }

        public static void OnException(Action<Exception> handler)
        {
            ExceptionHandler = handler;
        }

        public static void OnConnectionEvent(Action<string, object> handler)
        {
            ConnectionEventsHandler = handler;
        }

        internal static bool HasHadAConnection { set; get; }

        public SignalX()
        {
            Connections = new ConnectionMapping<string>();
            HubConfiguration = new HubConfiguration();
        }

        public static bool RequireAuthorizationForAllHandlers { get; set; }
        public static bool RequireAuthorizationForPersistentConnections { get; set; }

        public void Dispose()
        {
            MyApp.Dispose();
        }

        internal static ConcurrentDictionary<string, ServerHandlerDetails> _signalXServersDetails = new ConcurrentDictionary<string, ServerHandlerDetails>();

        internal static ConcurrentDictionary<string, Action<object, object, string, string, string, string>> _signalXServers = new ConcurrentDictionary<string, Action<object, object, string, string, string, string>>();

        public static void Server(string name, Action<object, object, string, string, string, string> server, bool requireAuthorization = false)
        {
            name = name.Trim();
            if (_signalXServers.ContainsKey(name) && !AllowDynamicServerInternal)
            {
                throw new Exception("Server with name '" + name + "' already created");
            }
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

        public static void RespondToAll(string name, object data)
        {
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
                return;
            }
            var hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<SignalXHub>();

            hubContext.Clients.All.broadcastMessage(name, data);
        }

        public static void RespondToUser(string userId, string name, object data)
        {
            if (!SignalX.HasHadAConnection)
            {
                SignalX.WarningHandler?.Invoke("DataSendingNotActivated",
                    new
                    {
                        UserId = userId,
                        Name = name,
                        Data = data,
                        Issue = "Cannot send message to client since no client connection has been obtained"
                    });
                return;
            }
            var hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<SignalXHub>();

            hubContext.Clients.User(userId).broadcastMessage(name, data);
        }
    }
}