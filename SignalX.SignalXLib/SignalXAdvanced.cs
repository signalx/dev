namespace SignalXLib.Lib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SignalXAdvanced
    {
        internal void Trace(Exception error,
            SignalXAdvancedLogType signalXAdvancedLogType,

            params object[] context)
        {
            this.Trace(signalXAdvancedLogType, error?.Message, error);
        }

        internal void Trace(
            Exception error,
            params object[] context)
        {
            this.Trace(SignalXAdvancedLogType.Exception, error?.Message, error);
        }

        internal void Trace(
            string message,

            params object[] context)
        {
            this.Trace(SignalXAdvancedLogType.Trace, message, null);
        }

        internal void Trace(Exception error,
            string message,

            params object[] context)
        {
            this.Trace(SignalXAdvancedLogType.Trace, message, error);
        }

        internal void Trace(
            SignalXAdvancedLogType signalXAdvancedLogType,
            string message,
            Exception error,
            params object[] context)
        {
            try
            {
                foreach (Action<SignalXAdvancedLogType, string, Exception, List<object>> traceHandler in this.TraceHandlers)
                {
                    try
                    {
                        traceHandler?.Invoke(signalXAdvancedLogType, message, error, context.ToList());
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void OnTrace(Action<SignalXAdvancedLogType, string, Exception, List<object>> logHandler)
        {
            TraceHandlers.Add(logHandler);
        }

        public void OnTrace(Action<SignalXAdvancedLogType, string, Exception> logHandler)
        {
            TraceHandlers.Add((s, m, e, l) => { logHandler?.Invoke(s, m, e); });
        }

        public void OnTrace(Action<string, Exception> logHandler)
        {
            TraceHandlers.Add((s, m, e, l) => { logHandler?.Invoke(m, e); });
        }
        
        public void OnTrace(Action<string> logHandler)
        {
            TraceHandlers.Add((s, m, e, l) => { logHandler?.Invoke(m); });
        }

        public SignalXAdvanced()
        {
            TraceHandlers = new List<Action<SignalXAdvancedLogType, string, Exception, List<object>>>();
        }

        internal List<Action<SignalXAdvancedLogType, string, Exception, List<object>>> TraceHandlers { set; get; }
    }
}