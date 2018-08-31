namespace SignalXLib.Lib
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Microsoft.AspNet.SignalR;

    public class SignalXSettings : IDisposable
    {
        internal string AuthenticationCookieName = "";
        internal long InComingCounter;
        internal long OutGoingCounter;

        public SignalXSettings()
        {
            this.HubConfiguration = new HubConfiguration { EnableDetailedErrors = true };
            this.Connections = new ConnectionMapping<string>();
            this.Receiver = new DefaultSignalRClientReceiver();
            this.SignalXServers = new ConcurrentDictionary<string, Action<SignalXRequest, SignalXServerState>>();
            this.SignalXClientDetails = new ConcurrentDictionary<string, ClientDetails>();
            this.SignalXServerExecutionDetails = new ConcurrentDictionary<string, ServerHandlerDetails>();
            this.ExceptionHandler = new List<Action<string, Exception>>();
            this.WarningHandler = new List<Action<string, object>>();
            this.ConnectionEventsHandler = new List<Action<string, object>>();
        }

        [Obsolete("Not intended for client access")]
        public ISignalXClientReceiver Receiver { set; get; }

        internal bool DisabledAllClients { set; get; }

        public HubConfiguration HubConfiguration { set; get; } //= new HubConfiguration() { EnableDetailedErrors = true };

        internal bool AllowDynamicServerInternal { set; get; }

        internal IDisposable MyApp { get; set; }

        internal Func<IRequest, bool> AuthenticatedWhen { set; get; }

        internal List<Action<string, object>> WarningHandler { set; get; }

        internal List<Action<string, object>> ConnectionEventsHandler { set; get; }

        internal List<Action<string, Exception>> ExceptionHandler { set; get; }

        /// <summary>
        ///     Is used only if SignalX.ManageUserConnections has been set to true, otherwise
        ///     Connections remains empty, even if there has been connections
        /// </summary>
        public ConnectionMapping<string> Connections { internal set; get; }

        public bool ManageUserConnections { set; get; }

        /// <summary>
        ///     Connections to signal X
        /// </summary>
        public bool HasOneOrMoreConnections { internal set; get; }

        public bool RequireAuthorizationForAllHandlers { get; set; }

        public bool RequireAuthorizationForPersistentConnections { get; set; }

        protected internal ConcurrentDictionary<string, Action<SignalXRequest, SignalXServerState>> SignalXServers { set; get; }

        internal ConcurrentDictionary<string, ClientDetails> SignalXClientDetails { set; get; }

        internal ConcurrentDictionary<string, ServerHandlerDetails> SignalXServerExecutionDetails { set; get; }

        internal bool StartCountingOutGoingMessages { set; get; }

        internal bool StartCountingInComingMessages { set; get; }

        public bool LogAgentMessagesOnClient { set; get; }

        //todo make this allow for multiple instances
        internal Action<dynamic, SignalXRequest, string> OnResponseAfterScriptRuns { set; get; }

        public bool ReceiveErrorMessagesFromClient { get; set; }

        public bool ReceiveDebugMessagesFromClient { get; set; }

        public void Dispose()
        {
            this.MyApp?.Dispose();
            this.SignalXClientDetails = null;
            this.SignalXServerExecutionDetails = null;
        }
    }
}