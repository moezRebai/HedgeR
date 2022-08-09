namespace HedgeR.Shared.Streaming
{
    public interface IStreamingPublisher
    {
        Task PublishAsync<T>(string topic, T message) where T : class;
    }
}
