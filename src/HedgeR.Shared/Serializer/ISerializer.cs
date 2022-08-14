namespace HedgeR.Shared.Serializer
{
    public interface ISerializer
    {
        string Serialize<T>(T data) where T : class;

        T? Deserialise<T>(string data) where T : class;
    }
}