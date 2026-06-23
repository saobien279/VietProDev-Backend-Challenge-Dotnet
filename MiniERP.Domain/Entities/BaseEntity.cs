using System;

namespace MiniERP.Domain.Entities
{
    public abstract class BaseEntity<TId>
    {
        public TId Id { get; set; } = default!;
        
        // Audit Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }

        // Soft Delete Fields
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
