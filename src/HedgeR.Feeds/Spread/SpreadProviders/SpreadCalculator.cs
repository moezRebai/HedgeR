
using HedgeR.Shared.Models;
using HedgeR.Spread.Options;
using Microsoft.Extensions.Options;

public class SpreadCalculator
{
    private readonly SpreadOptions _options;

    public SpreadCalculator(IOptions<SpreadOptions> options)
    {
        _options = options.Value;
    }

    public CurrencyPairSpread Calculate(string pair, decimal spot, decimal spreadBps, long timestamp)
    {
        var effectiveSpreadBps = spreadBps > 0 ? spreadBps : _options.DefaultSpreadBps;

        var bid = spot * (1 - effectiveSpreadBps / 20000m);
        var ask = spot * (1 + effectiveSpreadBps / 20000m);

        return new CurrencyPairSpread(pair, bid, ask, timestamp);
    }
}
