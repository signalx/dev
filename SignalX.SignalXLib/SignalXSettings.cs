namespace SignalXLib.Lib
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNet.SignalR;

    public class SignalXSettings : IDisposable
    {
        internal string AuthenticationCookieName = "";

        public string ClientLogScriptFunction = "console.log";
        internal long InComingCounter;
        internal long OutGoingCounter;

        public SignalXSettings()
        {
            this.HubConfiguration = new HubConfiguration { EnableDetailedErrors = true };

            this.ExceptionHandler = new List<Action<string, Exception>>();
            this.WarningHandler = new List<Action<string, object>>();
            this.ConnectionEventsHandler = new List<Action<string, object>>();
        }

        internal bool DisabledAllClients { set; get; }

        public HubConfiguration HubConfiguration { set; get; } //= new HubConfiguration() { EnableDetailedErrors = true };

        internal bool AllowDynamicServerInternal { set; get; }

        internal IDisposable MyApp { get; set; }

        internal Func<SignalXRequest, bool> AuthenticatedWhen { set; get; }

        internal List<Action<string, object>> WarningHandler { set; get; }

        internal List<Action<string, object>> ConnectionEventsHandler { set; get; }

        internal List<Action<string, Exception>> ExceptionHandler { set; get; }

        public bool ManageUserConnections { set; get; }

        /// <summary>
        ///     Connections to signal X
        /// </summary>
        public bool HasOneOrMoreConnections { internal set; get; }

        public bool RequireAuthorizationForAllHandlers { get; set; }

        public bool RequireAuthorizationForPersistentConnections { get; set; }

        internal bool StartCountingOutGoingMessages { set; get; }

        internal bool StartCountingInComingMessages { set; get; }

        public bool LogAgentMessagesOnClient { set; get; }

        public bool ReceiveErrorMessagesFromClient { get; set; }

        public bool ReceiveDebugMessagesFromClient { get; set; }

        public bool ContinueClientExecutionWhenAnyServerOnClientReadyFails { get; set; }

        public void Dispose()
        {
            this.MyApp?.Dispose();
            //todo dispose properly
            //this.SignalXClientDetails = null;
            //this.SignalXServerExecutionDetails = null;
        }
    }
}