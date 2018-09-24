namespace SignalXLib.Lib
{
    using System;

    public class ResponseAfterScriptRuns
    {
        readonly SignalX SignalX = SignalX.Instance;

        public string CorrelationId { set; get; }

        public SignalXRequest Request { set; get; }

        public string Error { get; set; }

        public string MessageAsJsonString { get; set; }

        public T MessageAs<T>(string message = null)
        {
            try
            {
                this.SignalX.Advanced.Trace(this.CorrelationId, $"Deserializing message string {message ?? this.MessageAsJsonString} to type {typeof(T).FullName}...");

                if (string.IsNullOrEmpty(message ?? this.MessageAsJsonString))
                    return default(T);
                return this.SignalX.Serializer.DeserializeObject<T>(message ?? this.MessageAsJsonString, this.CorrelationId);
            }
            catch (Exception e)
            {
                this.SignalX.Advanced.Trace(this.CorrelationId, e, $"Error deserializing message string {message ?? this.MessageAsJsonString} to type {typeof(T).FullName}...");
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