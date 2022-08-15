using HedgeR.Shared.Entities;

namespace HedgeR.Spot.Entities
{
    public class CurrencyPair : IEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal? DefaultValue { get; set; }
    }
}
