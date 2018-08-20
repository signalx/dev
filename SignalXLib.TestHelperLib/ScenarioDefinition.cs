namespace SignalXLib.TestHelperLib
{
    using System;

    public class ScenarioDefinition
    {
        public ScenarioDefinition(string script, Action server, Action checks)
        {
            this.Script = script;
            this.Server = server;
            this.Checks = checks;
        }

        public string Script { get; private set; }
        public Action Server { get; private set; }
        public Action Checks { get; private set; }
    }
}