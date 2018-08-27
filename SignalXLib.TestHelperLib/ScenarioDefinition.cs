namespace SignalXLib.TestHelperLib
{
    using System;

    public class SignalXTestDefinition
    {
        public SignalXTestDefinition(
            string script,
            Action server,
            BrowserType browserType = BrowserType.Unknown,
            Action checks = null,
            TestEventHandler events = null, int numberOfClients=1)
        {
            this.Script = script;
            this.Server = server;
            this.Checks = checks;
            this.BrowserType = browserType;
            this.TestEvents = events;
            NumberOfClients = numberOfClients;
        }

        public TestEventHandler TestEvents { get; set; }
        public string Script { get; }

        public Action Server { get; }

        public Action Checks { get; }

        public BrowserType BrowserType { get; }

        public int NumberOfClients { get; set; }
    }
}