namespace HedgeR.Shared.Streaming
{
    public interface IStreamingSubscriber
    {
        Task SubscribeAsync<T>(string topic, Action<T> handler) where T : class;
    }
}
