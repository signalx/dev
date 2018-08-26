namespace SignalXLib.TestHelperLib
{
    using System;

    public class TestEventHandler
    {
        public Action OnAppStarted { set; get; }

        public TestEventHandler(Action onClientLoaded = null, Action<Exception> onClientError = null, Action onCheckSucceeded = null, Action<Exception> onCheckFailures = null)
        {
            this.OnClientError = onClientError;
            this.OnClientLoaded = onClientLoaded;
            this.OnCheckSucceeded = onCheckSucceeded;
            this.OnCheckFailures = onCheckFailures;
        }

        public Action OnClientLoaded { get; }

        /// <summary>
        ///     Called when check succeeds
        /// </summary>
        public Action OnCheckSucceeded { get; }

        /// <summary>
        ///     Called, possibly many times after each attempt to try check fails
        /// </summary>
        public Action<Exception> OnCheckFailures { get; }

        public Action<Exception> OnClientError { get; }

        public Action<Exception> OnFinally { get; set; }
    }
}