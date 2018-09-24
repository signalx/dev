namespace SignalXLib.Lib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SignalXAdvanced
    {
        public SignalXAdvanced()
        {
            this.TraceHandlers = new List<Action<string,SignalXAdvancedLogType, string, Exception, List<object>>>();
        }

        internal List<Action<string,SignalXAdvancedLogType, string, Exception, List<object>>> TraceHandlers { set; get; }

        internal void Trace(string correlationId,
            Exception error,
            SignalXAdvancedLogType signalXAdvancedLogType,
            params object[] context)
        {
            this.Trace(correlationId, signalXAdvancedLogType, error?.Message, error, context);
        }

        internal void Trace(string correlationId,
            Exception error,
            params object[] context)
        {
            this.Trace(correlationId, SignalXAdvancedLogType.Exception, error?.Message, error, context);
        }

        internal void Trace(string correlationId,
            string message,
            params object[] context)
        {
            this.Trace(correlationId, SignalXAdvancedLogType.Trace, message, null,context);
        }

        internal void Trace(string correlationId,
            Exception error,
            string message,
            params object[] context)
        {
            this.Trace(correlationId, SignalXAdvancedLogType.Trace, message, error,context);
        }

        internal void Trace(string correlationId,
            SignalXAdvancedLogType signalXAdvancedLogType,
            string message,
            Exception error,
            params object[] context)
        {
            try
            {
                foreach (Action<string,SignalXAdvancedLogType, string, Exception, List<object>> traceHandler in this.TraceHandlers)
                    try
                    {
                        traceHandler?.Invoke(correlationId, signalXAdvancedLogType, message, error, context.ToList());
                    }
                    catch (Exception e)
                    {
                    }
            }
            catch (Exception e)
            {
               
            }
        }

        public void OnTrace(Action<string,SignalXAdvancedLogType, string, Exception, List<object>> logHandler)
        {
            this.TraceHandlers.Add(logHandler);
        }

        public void OnTrace(Action<string,SignalXAdvancedLogType, string, Exception> logHandler)
        {
            this.TraceHandlers.Add((c,s, m, e, l) => { logHandler?.Invoke(c,s, m, e); });
        }

        public void OnTrace(Action<string, Exception> logHandler)
        {
            this.TraceHandlers.Add((c,s, m, e, l) => { logHandler?.Invoke(m, e); });
        }

        public void OnTrace(Action<string> logHandler)
        {
            this.TraceHandlers.Add((c,s, m, e, l) => { logHandler?.Invoke(m); });
        }
        public void OnTraceWithCorrelation(Action<string,string> logHandler)
        {
            this.TraceHandlers.Add((c, s, m, e, l) => { logHandler?.Invoke(c,m); });
        }
    }
}