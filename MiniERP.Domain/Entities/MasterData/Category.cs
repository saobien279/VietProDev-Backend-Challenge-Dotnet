using System;
using System.Collections.Generic;

namespace MiniERP.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = null!;
        
        // Self-referencing (Danh mục Cha - Con)
        public int? ParentId { get; set; }
        public virtual Category? ParentCategory { get; set; }
        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}