namespace SignalXLib.TestHelperLib
{
    using System;

    public class SignalXTestDefinition
    {
        public SignalXTestDefinition(
            string script="",
            Action onAppStarted=null,
            Action checks = null,
            TestEventHandler events = null, 
            int numberOfClients=1,
            BrowserType browserType = BrowserType.Default)
        {
            this.Script = script;
            this.OnAppStarted = onAppStarted;
            this.Checks = checks;
            this.BrowserType = browserType;
            this.TestEvents = events;
            NumberOfClients = numberOfClients;
        }

        /// <summary>
        /// Hook into the various events throughout the test session
        /// </summary>
        public TestEventHandler TestEvents { get; set; }
        public string Script { get; }

        /// <summary>
        /// This is called once after the web api app starts
        /// </summary>
        public Action OnAppStarted { get; set; }

        /// <summary>
        /// This could be called multiple times so as to retry assertions 
        /// </summary>
        public Action Checks { get; set; }

        public BrowserType BrowserType { get; set; }

        /// <summary>
        /// This specifies the number of clients/browsers that should be spun up and waited for after the app starts
        /// </summary>
        public int NumberOfClients { get; set; }
    }
}