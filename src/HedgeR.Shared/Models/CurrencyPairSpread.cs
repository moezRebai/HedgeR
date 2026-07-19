namespace HedgeR.Shared.Models
{
    public record CurrencyPairSpread(string Pair, decimal Bid, decimal Ask, long Timestamp);
}
