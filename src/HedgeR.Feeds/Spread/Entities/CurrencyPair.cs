using HedgeR.Shared.Entities;

namespace HedgeR.Spread.Entities
{
    public class CurrencyPair : IEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal? DefaultValue { get; set; }

        public decimal SpreadBps { get; set; }
    }
}
