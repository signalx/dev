namespace SignalXLib.TestHelperLib
{
    using System;

    public class ScenarioDefinition
    {
        public ScenarioDefinition(string script, Action server, Action checks, Action onClientLoaded=null, Action<Exception> onClientError = null)
        {
            this.Script = script;
            this.Server = server;
            this.Checks = checks;
            this.OnClientError = onClientError;
            this.OnClientLoaded = onClientLoaded;
        }

        public string Script { get; private set; }
        public Action Server { get; private set; }
        public Action OnClientLoaded { get; private set; }
        public Action Checks { get; private set; }

       public  Action<Exception> OnClientError { private set; get; }
    }
}