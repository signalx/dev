using System;
using System.Collections.Concurrent;
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

        public IDisposable MyApp { get; set; }
       
        public void Dispose()
        {
            MyApp.Dispose();
        }

        internal static ConcurrentDictionary<string, Action<object, object, string, string>> _signalXServers = new ConcurrentDictionary<string, Action<object, object, string, string>>();

        public static void Server(string name, Action<object, object,string, string> server)
        {
            if (_signalXServers.ContainsKey(name))
            {
                throw new Exception("Server with name '" + name + "' already created");
            }
            var added = _signalXServers.TryAdd(name, server);
            var camelCased = Char.ToLowerInvariant(name[0]) + name.Substring(1);
            if (!_signalXServers.ContainsKey(camelCased))
            {
                added = _signalXServers.TryAdd(camelCased, server);//&&added;
            }

            var unCamelCased = Char.ToUpperInvariant(name[0]) + name.Substring(1);
            if (!_signalXServers.ContainsKey(unCamelCased))
            {
                added = _signalXServers.TryAdd(unCamelCased, server);//&&added;
            }
            
        }

        public static void Server(string name, Action<object, object,string> server)
        {
            Server(name, (message, sender, replyTo, messageId) => server(message, sender, replyTo));
        }
        public static void Server(string name, Action<object, object> server)
        {
            Server(name, (message,sender, replyTo,messageId) =>server(message, sender));
        }

        public static void Server(string name, Action<SignalXRequest> server)
        {
            Server(name, (message, sender, replyTo, messageId) => server(new SignalXRequest(replyTo, sender, messageId, message)));
        }

        public static void RespondTo(string name, object data)
        {
            var hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<SignalXHub>();

            hubContext.Clients.All.broadcastMessage(name, data);
        }
    }
}