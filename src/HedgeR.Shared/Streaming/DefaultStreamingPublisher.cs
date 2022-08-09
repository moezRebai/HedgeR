namespace HedgeR.Shared.Streaming
{
    public class DefaultStreamingPublisher : IStreamingPublisher
    {
        public Task PublishAsync<T>(string topic, T message) where T : class
        {
            return Task.CompletedTask;
        }
    }
}
