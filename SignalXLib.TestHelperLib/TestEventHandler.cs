namespace SignalXLib.TestHelperLib
{
    using System;

    public class TestEventHandler
    {
        internal Action OnAppStarted { set; get; }

        public TestEventHandler(Action onClientLoaded = null, Action<Exception> onClientError = null, Action onCheckSucceeded = null, Action<Exception> onCheckFailures = null)
        {
            this.OnClientError = onClientError;
            this.OnClientLoaded = onClientLoaded;
            this.OnCheckSucceeded = onCheckSucceeded;
            this.OnCheckFailures = onCheckFailures;
        }
        /// <summary>
        /// This is called once after all the clients have been loaded and verifies
        /// </summary>
        public Action OnClientLoaded { get; set; }

        /// <summary>
        /// This is called once when test succeeds
        /// </summary>
        public Action OnCheckSucceeded { get; set; }

        /// <summary>
        /// This is called, possibly, many times after each attempt to try check fails
        /// </summary>
        public Action<Exception> OnCheckFailures { get; set; }

        /// <summary>
        /// This is called once after the test fails
        /// </summary>
        public Action<Exception> OnClientError { get; set; }

        /// <summary>
        /// This is called no matter what, whether test succeeds or fails
        /// </summary>
        public Action<Exception> OnFinally { get; set; }
    }
}