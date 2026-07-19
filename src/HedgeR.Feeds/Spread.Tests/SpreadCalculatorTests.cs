using HedgeR.Shared.Models;
using HedgeR.Spread.Options;
using Microsoft.Extensions.Options;
using Xunit;

namespace HedgeR.Feeds.Spread.Tests
{
    public class SpreadCalculatorTests
    {
        private static SpreadCalculator CreateCalculator(decimal defaultSpreadBps = 20m)
        {
            var options = Options.Create(new SpreadOptions { DefaultSpreadBps = defaultSpreadBps });
            return new SpreadCalculator(options);
        }

        [Fact]
        public void Calculate_WithExplicitSpreadBps_ProducesExactBidAskPerFormula()
        {
            var calculator = CreateCalculator();

            var result = calculator.Calculate("EURUSD", 100m, 50m, 12345L);

            Assert.Equal("EURUSD", result.Pair);
            Assert.Equal(99.75m, result.Bid);
            Assert.Equal(100.25m, result.Ask);
            Assert.Equal(12345L, result.Timestamp);
        }

        [Theory]
        [InlineData(1.2345, 10)]
        [InlineData(1.2345, 500)]
        public void Calculate_WithExplicitSpreadBps_MatchesFormulaExactly(double spotDouble, double bpsDouble)
        {
            var spot = (decimal)spotDouble;
            var bps = (decimal)bpsDouble;
            var calculator = CreateCalculator();

            var result = calculator.Calculate("GBPUSD", spot, bps, 0L);

            Assert.Equal(spot * (1 - bps / 20000m), result.Bid);
            Assert.Equal(spot * (1 + bps / 20000m), result.Ask);
        }

        [Fact]
        public void Calculate_WithExplicitPositiveSpreadBps_UsesExplicitValueNotDefault()
        {
            var calculator = CreateCalculator(defaultSpreadBps: 999m);

            var result = calculator.Calculate("EURUSD", 100m, 25m, 0L);

            Assert.Equal(100m * (1 - 25m / 20000m), result.Bid);
            Assert.Equal(100m * (1 + 25m / 20000m), result.Ask);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Calculate_WithNonPositiveSpreadBps_FallsBackToConfiguredDefault(decimal spreadBps)
        {
            var calculator = CreateCalculator(defaultSpreadBps: 40m);

            var result = calculator.Calculate("EURUSD", 100m, spreadBps, 0L);

            Assert.Equal(100m * (1 - 40m / 20000m), result.Bid);
            Assert.Equal(100m * (1 + 40m / 20000m), result.Ask);
        }

        [Theory]
        [InlineData(0.00000001, 0.01)]
        [InlineData(0.00000001, 500)]
        [InlineData(1, 0.01)]
        [InlineData(1, 500)]
        [InlineData(1000000000, 0.01)]
        [InlineData(1000000000, 500)]
        public void Calculate_ForBoundarySpotAndBpsValues_BidIsLessThanSpotIsLessThanAsk(double spotDouble, double bpsDouble)
        {
            var spot = (decimal)spotDouble;
            var bps = (decimal)bpsDouble;
            var calculator = CreateCalculator();

            var result = calculator.Calculate("EURUSD", spot, bps, 0L);

            Assert.True(result.Bid < spot);
            Assert.True(spot < result.Ask);
        }

        [Theory]
        [InlineData(0.00000001)]
        [InlineData(1)]
        [InlineData(1000000000)]
        public void Calculate_ForBoundarySpotValues_WithDefaultFallback_BidIsLessThanSpotIsLessThanAsk(double spotDouble)
        {
            var spot = (decimal)spotDouble;
            var calculator = CreateCalculator(defaultSpreadBps: 15m);

            var result = calculator.Calculate("EURUSD", spot, 0m, 0L);

            Assert.True(result.Bid < spot);
            Assert.True(spot < result.Ask);
        }
    }
}
