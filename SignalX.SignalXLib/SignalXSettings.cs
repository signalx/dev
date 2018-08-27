namespace SignalXLib.Lib
{
    using System;
    using System.Collections.Concurrent;
    using Microsoft.AspNet.SignalR;

   
    public class SignalXSettings:IDisposable
    {
      
        public SignalXSettings()
        {
            HubConfiguration = new HubConfiguration() { EnableDetailedErrors = true };
            Connections = new ConnectionMapping<string>();
            Receiver = new DefaultSignalRClientReceiver();
            SignalXServers = new ConcurrentDictionary<string, Action<SignalXRequest, SignalXServerState>>();
            SignalXClientDetails = new ConcurrentDictionary<string, ClientDetails>();
            SignalXServerExecutionDetails = new ConcurrentDictionary<string, ServerHandlerDetails>();
        }

        public ISignalXClientReceiver Receiver { set; get; }

        internal  bool DisabledAllClients { set; get; }

        public HubConfiguration HubConfiguration { set; get; } //= new HubConfiguration() { EnableDetailedErrors = true };

        internal  bool AllowDynamicServerInternal { set; get; }
        public IDisposable MyApp { get; set; }
        internal  string AuthenticationCookieName = "";
        internal Func<IRequest, bool> AuthenticatedWhen { set; get; }
        internal  Action<string, object> WarningHandler { set; get; }
        internal  Action<string, object> ConnectionEventsHandler { set; get; }
        /// <summary>
        /// Is used only if SignalX.ManageUserConnections has been set to true, otherwise
        /// Connections remains empty, even if there has been connections
        /// </summary>
        public  ConnectionMapping<string> Connections { set; get; }
        public  bool ManageUserConnections { set; get; }
        internal  Action<string, Exception> ExceptionHandler { set; get; }
        public  bool HasOneOrMoreConnections { set; get; }
        public  bool RequireAuthorizationForAllHandlers { get; set; }
        public  bool RequireAuthorizationForPersistentConnections { get; set; }

        protected internal ConcurrentDictionary<string, Action<SignalXRequest, SignalXServerState>> SignalXServers { set; get; }

        internal  ConcurrentDictionary<string, ClientDetails> SignalXClientDetails { set; get; }
        internal  ConcurrentDictionary<string, ServerHandlerDetails> SignalXServerExecutionDetails { set; get; }
        internal  bool StartCountingOutGoingMessages { set; get; }
        internal  bool StartCountingInComingMessages { set; get; }
        internal  long OutGoingCounter;
        internal  long InComingCounter;
        public  bool LogAgentMessagesOnClient { set; get; }
        //todo make this allow for multiple instances
        internal  Action<dynamic, SignalXRequest, string> OnResponseAfterScriptRuns { set; get; }

       

        public void Dispose()
        {
            this.MyApp?.Dispose();
            SignalXClientDetails = null;
            SignalXServerExecutionDetails = null;
        }
    }
}