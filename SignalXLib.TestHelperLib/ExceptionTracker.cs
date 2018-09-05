namespace SignalXLib.TestHelperLib
{
    using System;

    public class ExceptionTracker
    {
        public Exception Exception { get; set; }
        public string Context { set; get; }
    }
}