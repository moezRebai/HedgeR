using HedgeR.Shared.Serializer;
using HedgeR.Shared.Streaming;
using StackExchange.Redis;

namespace HedgeR.Shared.Redis.Streaming
{
    internal class RedisStreamingPublisher : IStreamingPublisher
    {
        private readonly ISubscriber _subscriber;
        private readonly ISerializer _serializer;

        public RedisStreamingPublisher(IConnectionMultiplexer connectionMultiplexer, ISerializer serializer)
        {
            _subscriber = connectionMultiplexer.GetSubscriber();
            _serializer = serializer;
        }

        public Task PublishAsync<T>(string topic, T message) where T : class
        {
            var payload = _serializer.Serialize(message);

            return _subscriber.PublishAsync(topic, payload);
        }
    }
}
