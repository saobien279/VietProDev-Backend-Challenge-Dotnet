using System;
using System.Collections.Generic;

namespace MiniERP.Domain.Entities
{
    public class Unit : BaseEntity<int>
    {
        public string UnitName { get; set; } = null!; // Cái, Thùng, Hộp...

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}