using System.Linq.Expressions;
using HedgeR.Shared.Entities;
using HedgeR.Shared.Models;
using HedgeR.Shared.Streaming;
using HedgeR.Spread.BackgroundServices;
using HedgeR.Spread.Entities;
using HedgeR.Spread.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace HedgeR.Feeds.Spread.Tests
{
    public class SpreadStreamProviderTests
    {
        private static SpreadStreamProvider CreateProvider(
            FakeStreamingSubscriber subscriber,
            FakeStreamingPublisher publisher,
            IReadOnlyCollection<CurrencyPair> currencyPairs,
            decimal defaultSpreadBps = 10m)
        {
            var calculator = new SpreadCalculator(Options.Create(new SpreadOptions { DefaultSpreadBps = defaultSpreadBps }));
            var repository = new FakeCurrencyPairRepository(currencyPairs);

            return new SpreadStreamProvider(new FakeLogger<SpreadStreamProvider>(), subscriber, publisher, calculator, repository);
        }

        [Fact]
        public async Task ExecuteAsync_SubscribesToSpotFeed()
        {
            var subscriber = new FakeStreamingSubscriber();
            var provider = CreateProvider(subscriber, new FakeStreamingPublisher(), Array.Empty<CurrencyPair>());

            await provider.StartAsync(CancellationToken.None);

            Assert.Equal("SpotFeed", subscriber.Topic);
        }

        [Fact]
        public async Task SpotTick_ForPairWithConfiguredSpread_PublishesSpreadFeedUsingConfiguredBps()
        {
            var subscriber = new FakeStreamingSubscriber();
            var publisher = new FakeStreamingPublisher();
            var provider = CreateProvider(subscriber, publisher, new[] { new CurrencyPair { Id = 1, Name = "EURUSD", SpreadBps = 50m } });
            await provider.StartAsync(CancellationToken.None);

            subscriber.Publish(new CurrencyPairSpot("EURUSD", 100m, 12345L));

            Assert.Equal("SpreadFeed", publisher.Topic);
            var spread = Assert.IsType<CurrencyPairSpread>(publisher.Message);
            Assert.Equal("EURUSD", spread.Pair);
            Assert.Equal(100m * (1 - 50m / 20000m), spread.Bid);
            Assert.Equal(100m * (1 + 50m / 20000m), spread.Ask);
            Assert.Equal(12345L, spread.Timestamp);
        }

        [Fact]
        public async Task SpotTick_ForPairWithNoConfiguredSpread_FallsBackToConfiguredDefault()
        {
            var subscriber = new FakeStreamingSubscriber();
            var publisher = new FakeStreamingPublisher();
            var provider = CreateProvider(subscriber, publisher, new[] { new CurrencyPair { Id = 1, Name = "GBPUSD", SpreadBps = 0m } }, defaultSpreadBps: 15m);
            await provider.StartAsync(CancellationToken.None);

            subscriber.Publish(new CurrencyPairSpot("GBPUSD", 200m, 999L));

            var spread = Assert.IsType<CurrencyPairSpread>(publisher.Message);
            Assert.Equal("GBPUSD", spread.Pair);
            Assert.Equal(200m * (1 - 15m / 20000m), spread.Bid);
            Assert.Equal(200m * (1 + 15m / 20000m), spread.Ask);
        }

        [Fact]
        public async Task SpotTick_ForPairNotInCatalog_FallsBackToConfiguredDefault()
        {
            var subscriber = new FakeStreamingSubscriber();
            var publisher = new FakeStreamingPublisher();
            var provider = CreateProvider(subscriber, publisher, Array.Empty<CurrencyPair>(), defaultSpreadBps: 25m);
            await provider.StartAsync(CancellationToken.None);

            subscriber.Publish(new CurrencyPairSpot("AUDUSD", 50m, 1L));

            var spread = Assert.IsType<CurrencyPairSpread>(publisher.Message);
            Assert.Equal(50m * (1 - 25m / 20000m), spread.Bid);
            Assert.Equal(50m * (1 + 25m / 20000m), spread.Ask);
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

        private class FakeStreamingPublisher : IStreamingPublisher
        {
            public string? Topic { get; private set; }
            public object? Message { get; private set; }

            public Task PublishAsync<T>(string topic, T message) where T : class
            {
                Topic = topic;
                Message = message;
                return Task.CompletedTask;
            }
        }

        private class FakeCurrencyPairRepository : IRepository<CurrencyPair>
        {
            private readonly IReadOnlyCollection<CurrencyPair> _currencyPairs;

            public FakeCurrencyPairRepository(IReadOnlyCollection<CurrencyPair> currencyPairs)
            {
                _currencyPairs = currencyPairs;
            }

            public Task<IReadOnlyCollection<CurrencyPair>> GetAllAsync() => Task.FromResult(_currencyPairs);

            public Task<IReadOnlyCollection<CurrencyPair>> GetAllAsync(Expression<Func<CurrencyPair, bool>> filter) => throw new NotImplementedException();

            public Task<CurrencyPair> GetAsync(int id) => throw new NotImplementedException();

            public Task<CurrencyPair> GetAsync(Expression<Func<CurrencyPair, bool>> filter) => throw new NotImplementedException();

            public Task CreateAsync(CurrencyPair entity) => throw new NotImplementedException();

            public Task UpdateAsync(CurrencyPair entity) => throw new NotImplementedException();

            public Task RemoveAsync(int id) => throw new NotImplementedException();
        }

        private class FakeLogger<T> : ILogger<T>
        {
            public IDisposable BeginScope<TState>(TState state) where TState : notnull => null!;

            public bool IsEnabled(LogLevel logLevel) => false;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
            }
        }
    }
}
