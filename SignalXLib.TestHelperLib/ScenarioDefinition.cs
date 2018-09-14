namespace SignalXLib.TestHelperLib
{
    using System;
    using System.Collections.Generic;

    public class SignalXTestDefinition
    {
        public SignalXTestDefinition(
            string script,
            Action onAppStarted = null,
            Action checks = null,
            TestEventHandler events = null,
            BrowserType? browserType = null)
        {
            this.ConstructSignalXTestDefinition(
                new List<string> { script ?? "" },
                onAppStarted,
                checks,
                events,
                1,
                browserType);
        }

        public SignalXTestDefinition(
            string script,
            int numberOfClients,
            Action onAppStarted = null,
            Action checks = null,
            TestEventHandler events = null,
            BrowserType? browserType = null)
        {
            this.ConstructSignalXTestDefinition(
                new List<string> { script ?? "" },
                onAppStarted,
                checks,
                events,
                numberOfClients,
                browserType);
        }

        public SignalXTestDefinition(
            List<string> script = null,
            Action onAppStarted = null,
            Action checks = null,
            TestEventHandler events = null,
            BrowserType? browserType = null)
        {
            this.ConstructSignalXTestDefinition(
                script ?? new List<string> { "" },
                onAppStarted,
                checks,
                events,
                script?.Count ?? 1,
                browserType);
        }

        /// <summary>
        ///     Hook into the various events throughout the test session
        /// </summary>
        public TestEventHandler TestEvents { get; set; }

        public List<string> Script { get; internal set; }

        /// <summary>
        ///     This is called once after the web api app starts
        /// </summary>
        public Action OnAppStarted { get; set; }

        public Func<TestObject, TestObject> OnClientPrepared { get; set; }

        /// <summary>
        ///     This could be called multiple times so as to retry assertions
        /// </summary>
        public Action Checks { get; set; }

        public BrowserType BrowserType { get; set; }

        /// <summary>
        ///     This specifies the number of clients/browsers that should be spun up and waited for after the app starts
        /// </summary>
        public int NumberOfClients { get; internal set; }

        void ConstructSignalXTestDefinition(
            List<string> script,
            Action onAppStarted,
            Action checks,
            TestEventHandler events,
            int numberOfClients,
            BrowserType? browserType)
        {
            this.BrowserType = browserType ?? SignalXTester.DefaultBrowserType;
            script = script ?? new List<string> { "" };
            this.Script = script;
            this.OnAppStarted = onAppStarted;
            this.Checks = checks;
            this.TestEvents = events;
            this.NumberOfClients = numberOfClients;
        }
    }
}