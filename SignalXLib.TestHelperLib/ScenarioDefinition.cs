namespace SignalXLib.TestHelperLib
{
    using System;
    using System.Collections.Generic;

    public class SignalXTestDefinition
    {
        const BrowserType DefaultBrowserType = BrowserType.Default;
        static bool? _embedeLibraryScripts = null;
        public SignalXTestDefinition(
            string script ,
            Action onAppStarted = null,
            Action checks = null,
            TestEventHandler events = null,
            BrowserType browserType = DefaultBrowserType)
        {
            this.ConstructSignalXTestDefinition(
                new List<string>() { script??"" },
                onAppStarted,
                checks,
                events,
                1,
                browserType);
        }

        public SignalXTestDefinition(
            string script ,
            int numberOfClients,
            Action onAppStarted = null,
            Action checks = null,
            TestEventHandler events = null,
            
            BrowserType browserType = DefaultBrowserType)
        {
            ConstructSignalXTestDefinition(
                new List<string>(){ script??"" },
                onAppStarted,
                checks,
                events,
                numberOfClients,
                browserType);
        }

        public SignalXTestDefinition(
            List<string> script=null,
            Action onAppStarted=null,
            Action checks = null,
            TestEventHandler events = null,
            BrowserType browserType = DefaultBrowserType)
        {
            ConstructSignalXTestDefinition(
                script?? new List<string>(){""},
                onAppStarted,
                checks,
                events,
                script?.Count??1,
                browserType);
        }

        void ConstructSignalXTestDefinition(
            List<string> script,
            Action onAppStarted ,
            Action checks ,
            TestEventHandler events ,
            int numberOfClients ,
            BrowserType browserType )
        {
            script = script ?? new List<string>() { "" };
            this.Script = script;
            this.OnAppStarted = onAppStarted;
            this.Checks = checks;
            this.BrowserType = browserType;
            this.TestEvents = events;
            NumberOfClients = numberOfClients;

            EmbedeLibraryScripts = _embedeLibraryScripts ?? EmbedeLibraryScripts;
        }
        /// <summary>
        /// Hook into the various events throughout the test session
        /// </summary>
        public TestEventHandler TestEvents { get; set; }
        public List<string> Script { get; internal set; }

        /// <summary>
        /// This is called once after the web api app starts
        /// </summary>
        public Action OnAppStarted { get; set; }
        public Func<TestObject, TestObject> OnClientPrepared { get; set; }

        /// <summary>
        /// This could be called multiple times so as to retry assertions 
        /// </summary>
        public Action Checks { get; set; }

        public BrowserType BrowserType { get; set; }

        /// <summary>
        /// This specifies the number of clients/browsers that should be spun up and waited for after the app starts
        /// </summary>
        public int NumberOfClients { get; internal set; }

        public bool EmbedeLibraryScripts { get; set; }
    }
}