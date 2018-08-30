namespace SignalXLib.Lib
{
    public class ResponseAfterScriptRuns
    {
        public string Error { get; set; }

        public dynamic Result { get; set; }
    }

    public class ResponseFromClientError
    {
        public string description { get; set; }

        public string error { get; set; }
    }

    public class ResponseFromClientDebug
    {
        public string description { get; set; }

        public string error { get; set; }
    }
}