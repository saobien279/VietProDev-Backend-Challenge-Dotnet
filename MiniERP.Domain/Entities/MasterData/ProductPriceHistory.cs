using MiniERP.Domain.Enums;
using System;

namespace MiniERP.Domain.Entities
{
    public class ProductPriceHistory : BaseEntity<Guid>
    {
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        public PriceType PriceType { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public string? Note { get; set; }
    }
}
