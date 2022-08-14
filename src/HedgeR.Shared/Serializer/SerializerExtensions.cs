using Microsoft.Extensions.DependencyInjection;

namespace HedgeR.Shared.Serializer
{
    public static class SerializerExtensions
    {
        public static IServiceCollection AddSerializer(this IServiceCollection services)
        {
            return services.AddSingleton<ISerializer, DefaultSerializer>();
        }
    }
}
