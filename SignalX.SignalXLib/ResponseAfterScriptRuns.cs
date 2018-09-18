namespace SignalXLib.Lib
{
    using System;

    public class ResponseAfterScriptRuns
    {
        SignalX SignalX= Lib.SignalX.Instance;
        
        public SignalXRequest Request { set; get; }
        public string Error { get; set; }

        public string MessageAsJsonString { get; set; }
        public T MessageAs<T>(string message = null)
        {
            try
            {
                SignalX.Advanced.Trace($"Deserializing message string {message ?? this.MessageAsJsonString} to type {typeof(T).FullName}...");

                if (string.IsNullOrEmpty(message ?? this.MessageAsJsonString))
                {
                    return default(T);
                }
                return SignalXExtensions.Serializer.DeserializeObject<T>(message ?? this.MessageAsJsonString);
            }
            catch (Exception e)
            {
                SignalX.Advanced.Trace(e, $"Error deserializing message string {message ?? this.MessageAsJsonString} to type {typeof(T).FullName}...");
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