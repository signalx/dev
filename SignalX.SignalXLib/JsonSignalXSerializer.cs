namespace SignalXLib.Lib
{
    using Newtonsoft.Json;

    public class JsonSignalXSerializer : ISignalXSerializer
    {
        private JsonSerializerSettings JsonSerializerSettings { set; get; }

        public JsonSignalXSerializer(JsonSerializerSettings jsonSerializerSettings = null)
        {
            this.JsonSerializerSettings = jsonSerializerSettings ?? new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        public T DeserializeObject<T>(string data, string correlationId)
        {
            return JsonConvert.DeserializeObject<T>(data, this.JsonSerializerSettings);
        }

        public string SerializeObject(object data, string correlationId)
        {
            return JsonConvert.SerializeObject(data, this.JsonSerializerSettings);
        }
    }
}