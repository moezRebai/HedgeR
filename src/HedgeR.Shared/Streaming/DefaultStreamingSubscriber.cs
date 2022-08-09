namespace HedgeR.Shared.Streaming
{
    public class DefaultStreamingSubscriber : IStreamingSubscriber
    {
        public Task SubscribeAsync<T>(string topic, Action<T> handler) where T : class
        {
            return Task.CompletedTask;
        }
    }
}
