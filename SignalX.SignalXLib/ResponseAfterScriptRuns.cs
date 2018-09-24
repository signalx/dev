namespace SignalXLib.Lib
{
    using System;

    public class ResponseAfterScriptRuns
    {
        private SignalX SignalX = Lib.SignalX.Instance;
        public string CorrelationId { set; get; }
        public SignalXRequest Request { set; get; }
        public string Error { get; set; }

        public string MessageAsJsonString { get; set; }

        public T MessageAs<T>(string message = null)
        {
            try
            {
                SignalX.Advanced.Trace(CorrelationId, $"Deserializing message string {message ?? this.MessageAsJsonString} to type {typeof(T).FullName}...");

                if (string.IsNullOrEmpty(message ?? this.MessageAsJsonString))
                {
                    return default(T);
                }
                return SignalX.Serializer.DeserializeObject<T>(message ?? this.MessageAsJsonString, CorrelationId);
            }
            catch (Exception e)
            {
                SignalX.Advanced.Trace(CorrelationId, e, $"Error deserializing message string {message ?? this.MessageAsJsonString} to type {typeof(T).FullName}...");
                this.SignalX.Settings.ExceptionHandler.ForEach(h => h?.Invoke("MessageSerializationError", e));

                return default(T);
            }
        }
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