using HedgeR.Shared.Streaming;
using Microsoft.Extensions.DependencyInjection;

namespace HedgeR.Shared.Redis.Streaming
{
    public static class RedisStreamingExtension
    {
        public static IServiceCollection AddRedisStreaming(this IServiceCollection services)
        {
            return services.AddSingleton<IStreamingPublisher, RedisStreamingPublisher>()
                .AddSingleton<IStreamingSubscriber, RedisStreamingSubscriber>();
        }
    }
}
