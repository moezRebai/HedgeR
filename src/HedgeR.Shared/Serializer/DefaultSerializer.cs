using System.Text.Json;

namespace HedgeR.Shared.Serializer
{
    public class DefaultSerializer : ISerializer
    {
        readonly JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };


        public T? Deserialise<T>(string data) where T : class
        {
            return JsonSerializer.Deserialize<T>(data, options);
        }

        public string Serialize<T>(T data) where T : class
        {
            return JsonSerializer.Serialize<T>(data, options);
        }
    }
}