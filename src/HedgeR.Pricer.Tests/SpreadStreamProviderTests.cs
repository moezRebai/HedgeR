using HedgeR.Pricer.StreamProviders;
using HedgeR.Shared.Models;
using HedgeR.Shared.Streaming;
using Microsoft.Extensions.Logging;
using Xunit;

namespace HedgeR.Pricer.Tests
{
    public class SpreadStreamProviderTests
    {
        [Fact]
        public async Task ExecuteAsync_SubscribesToSpreadFeed()
        {
            var subscriber = new FakeStreamingSubscriber();
            var provider = new SpreadStreamProvider(new FakeLogger<SpreadStreamProvider>(), subscriber);

            await provider.StartAsync(CancellationToken.None);

            Assert.Equal("SpreadFeed", subscriber.Topic);
        }

        [Fact]
        public async Task SpreadTick_LogsMessageDistinctFromSpotTickLog()
        {
            var subscriber = new FakeStreamingSubscriber();
            var logger = new FakeLogger<SpreadStreamProvider>();
            var provider = new SpreadStreamProvider(logger, subscriber);
            await provider.StartAsync(CancellationToken.None);

            subscriber.Publish(new CurrencyPairSpread("EURUSD", 99.75m, 100.25m, 12345L));

            var message = Assert.Single(logger.Messages);
            Assert.Contains("spread", message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("bid", message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("ask", message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("99.75", message);
            Assert.Contains("100.25", message);
            Assert.Contains("12345", message);
            Assert.DoesNotContain("wih value", message);
        }

        private class FakeStreamingSubscriber : IStreamingSubscriber
        {
            public string? Topic { get; private set; }
            private object? _handler;

            public Task SubscribeAsync<T>(string topic, Action<T> handler) where T : class
            {
                Topic = topic;
                _handler = handler;
                return Task.CompletedTask;
            }

            public void Publish<T>(T message) where T : class
            {
                ((Action<T>)_handler!)(message);
            }
        }

        private class FakeLogger<T> : ILogger<T>
        {
            public List<string> Messages { get; } = new();

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => null!;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                Messages.Add(formatter(state, exception));
            }
        }
    }
}
