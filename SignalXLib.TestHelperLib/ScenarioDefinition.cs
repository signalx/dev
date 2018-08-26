namespace SignalXLib.TestHelperLib
{
    using System;

    public class SignalXTestDefinition : SignalXTestDefinition<object>
    {
        public SignalXTestDefinition( string script, Action server, Action checks, Action onClientLoaded = null, Action<Exception> onClientError = null, BrowserType browserType = BrowserType.Unknown, Action onCheckSucceeded = null, Action<Exception> onCheckFailures = null)
            : base(null, script, server, (o)=>{ checks?.Invoke();}, onClientLoaded, onClientError, browserType, onCheckSucceeded, onCheckFailures)
        {
        }
    }
    public class SignalXTestDefinition<TE>
    {
        public SignalXTestDefinition(TE expectation,string script, Action server, Action<TE> checks, Action onClientLoaded=null, Action<Exception> onClientError = null, BrowserType browserType = BrowserType.Unknown, Action onCheckSucceeded=null, Action<Exception> onCheckFailures = null)
        {
            this.Script = script;
            this.Server = server;
            this.Checks = checks;
            this.BrowserType = browserType;
            this.OnClientError = onClientError;
            this.OnClientLoaded = onClientLoaded;
            this.OnCheckSucceeded = onCheckSucceeded;
            this.OnCheckFailures = onCheckFailures;
            Expectation = expectation;
        }

       public  TE Expectation { set; get; }
        public string Script { get; private set; }
        public Action Server { get; private set; }
        public Action OnClientLoaded { get; private set; }
        public Action<TE> Checks { get; private set; }

        /// <summary>
        /// Called when check succeeds
        /// </summary>
        public Action OnCheckSucceeded { get; private set; }
        /// <summary>
        /// Called, possibly many times after each attempt to try check fails
        /// </summary>
        public Action<Exception> OnCheckFailures { get; private set; }

        public  Action<Exception> OnClientError { private set; get; }

        public BrowserType BrowserType { get; private set; }
    }
}