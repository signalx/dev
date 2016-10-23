using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;


namespace SignalXLib.Lib
{
    public class SignalX : IDisposable
    {
        internal  string UiFolder { set; get; }
        internal  string BaseUiDirectory = AppDomain.CurrentDomain.BaseDirectory;

        public  string UiDirectory => BaseUiDirectory + UiFolder;
        internal static Action<Exception> ExceptionHandler { set; get; }

        public static  ConnectionMapping<string> Connections { set; get; }

        public static void OnException(Action<Exception> handler)
        {
            ExceptionHandler = handler;
        }

        public SignalX( string uiFolder, string baseUiDirectory=null)
        {
            UiFolder = uiFolder;
            BaseUiDirectory = baseUiDirectory ?? BaseUiDirectory;
            HubConfiguration = new HubConfiguration();
        }

        public SignalX(HubConfiguration hubConfiguration= null 
            , string uiFolder=null
            , string baseUiDirectory = null
            )
        {
            if (hubConfiguration == null) throw new ArgumentNullException(nameof(hubConfiguration));
            HubConfiguration = hubConfiguration;
            UiFolder = uiFolder;
            BaseUiDirectory = baseUiDirectory ?? BaseUiDirectory;
        }

        public  HubConfiguration HubConfiguration { get; set; }
        internal static bool AllowDynamicServerInternal { set; get; }
        public  bool AllowDynamicServer {
            set { AllowDynamicServer = value; }
        }
        public IDisposable MyApp { get; set; }
       
        public void Dispose()
        {
            MyApp.Dispose();
        }

        internal static ConcurrentDictionary<string, Action<object, object, string, string,string,string>> _signalXServers = new ConcurrentDictionary<string, Action<object, object, string, string,string,string>>();

        public static void Server(string name, Action<object, object,string, string,string,string> server)
        {
            
            if (_signalXServers.ContainsKey(name) && !AllowDynamicServerInternal)
            {
                throw new Exception("Server with name '" + name + "' already created");
            }
             _signalXServers[name]= server;
            var camelCased = Char.ToLowerInvariant(name[0]) + name.Substring(1);
               _signalXServers[camelCased]= server; //&&added;
            var unCamelCased = Char.ToUpperInvariant(name[0]) + name.Substring(1);
              _signalXServers[unCamelCased]= server;//&&added;
        }


        public static void Server(string name, Action<object, object, string,string,string> server)
        {
            Server(name, (message, sender, replyTo, messageId, userId, connectionId) => server(message, sender, replyTo,userId,connectionId));
        }
        public static void Server(string name, Action<object, object, string,string> server)
        {
            Server(name, (message, sender, replyTo, messageId, userId, connectionId) => server(message, sender, replyTo,userId));
        }
        public static void Server(string name, Action<object, object,string> server)
        {
            Server(name, (message, sender, replyTo, messageId, userId, connectionId) => server(message, sender, replyTo));
        }
        public static void Server(string name, Action<object, object> server)
        {
            Server(name, (message,sender, replyTo,messageId, userId, connectionId) =>server(message, sender));
        }

        public static void Server(string name, Action<SignalXRequest> server)
        {
            Server(name, (message, sender, replyTo, messageId, userId, connectionId) => server(new SignalXRequest(replyTo, sender, messageId, message, userId, connectionId)));
        }

        public static void RespondToAll(string name, object data)
        {
            var hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<SignalXHub>();

            hubContext.Clients.All.broadcastMessage(name, data);
        }
        public static void RespondToUser( string userId,string name, object data)
        {
            var hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<SignalXHub>();

            hubContext.Clients.User(userId).broadcastMessage(name, data);
        }
    }

    public class ConnectionMapping<T>
    {
        private readonly Dictionary<T, HashSet<string>> _connections = new Dictionary<T, HashSet<string>>();

        public int Count
        {
            get
            {
                return _connections.Count;
            }
        }

        public void Add(T key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    connections = new HashSet<string>();
                    _connections.Add(key, connections);
                }

                lock (connections)
                {
                    connections.Add(connectionId);
                }
            }
        }

        public IEnumerable<string> GetConnections(T key)
        {
            HashSet<string> connections;
            if (_connections.TryGetValue(key, out connections))
            {
                return connections;
            }

            return Enumerable.Empty<string>();
        }

        public void Remove(T key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    return;
                }

                lock (connections)
                {
                    connections.Remove(connectionId);

                    if (connections.Count == 0)
                    {
                        _connections.Remove(key);
                    }
                }
            }
        }
    }
}