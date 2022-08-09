using Microsoft.Extensions.DependencyInjection;

namespace HedgeR.Shared.Streaming
{
    public static class StreamingExtension
    {
        public static IServiceCollection AddStreaming(this IServiceCollection services)
            => services.AddSingleton<IStreamingPublisher, DefaultStreamingPublisher>()
            .AddSingleton<IStreamingSubscriber, DefaultStreamingSubscriber>();
    }
}
