namespace SignalXLib.Lib
{
    public interface ISignalXSerializer
    {
        T DeserializeObject<T>(string data, string correlationId);

        string SerializeObject(object data, string correlationId);
    }
}