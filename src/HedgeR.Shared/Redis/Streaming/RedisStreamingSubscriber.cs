using HedgeR.Shared.Serializer;
using HedgeR.Shared.Streaming;
using StackExchange.Redis;

namespace HedgeR.Shared.Redis.Streaming
{
    internal class RedisStreamingSubscriber : IStreamingSubscriber
    {
        private readonly ISubscriber _subscriber;
        private readonly ISerializer _serializer;

        public RedisStreamingSubscriber(IConnectionMultiplexer connectionMultiplexer, ISerializer serializer)
        {
            _subscriber = connectionMultiplexer.GetSubscriber();
            _serializer = serializer;
        }

        public Task PublishAsync<T>(string topic, T message) where T : class
        {
            var payload = _serializer.Serialize(message);

            return _subscriber.PublishAsync(topic, payload);
        }

        public Task SubscribeAsync<T>(string topic, Action<T> handler) where T : class
        {
            return _subscriber.SubscribeAsync(topic, (_, message) =>
            {
                var payload = _serializer.Deserialise<T>(message!);

                if (payload == null)
                {
                    return;
                }

                handler(payload);
            });
        }
    }
}
