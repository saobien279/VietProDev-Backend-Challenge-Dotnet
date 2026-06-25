using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MiniERP.Domain.Entities;
using MiniERP.Application.Interfaces.Services;

namespace MiniERP.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ICurrentUserService currentUserService) : base(options)
        {
            _currentUserService = currentUserService;
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Unit> Units => Set<Unit>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Inventory> Inventories => Set<Inventory>();
        public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
        public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
        public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
        public DbSet<SalesOrderItem> SalesOrderItems => Set<SalesOrderItem>();
        public DbSet<Payment> Payments => Set<Payment>();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.UserId;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is BaseEntity<Guid> auditEntity)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntity.CreatedAt = DateTime.UtcNow;
                            auditEntity.CreatedBy = currentUserId;
                            break;

                        case EntityState.Modified:
                            entry.Property("CreatedAt").IsModified = false;
                            entry.Property("CreatedBy").IsModified = false;
                            entry.Property("DeletedAt").IsModified = false;
                            entry.Property("DeletedBy").IsModified = false;

                            auditEntity.UpdatedAt = DateTime.UtcNow;
                            auditEntity.UpdatedBy = currentUserId;
                            break;

                        case EntityState.Deleted:
                            entry.State = EntityState.Modified;
                            auditEntity.DeletedAt = DateTime.UtcNow;
                            auditEntity.DeletedBy = currentUserId;
                            break;
                    }
                }
                else if (entry.Entity is BaseEntity<int> intAuditEntity)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            intAuditEntity.CreatedAt = DateTime.UtcNow;
                            intAuditEntity.CreatedBy = currentUserId;
                            break;

                        case EntityState.Modified:
                            entry.Property("CreatedAt").IsModified = false;
                            entry.Property("CreatedBy").IsModified = false;
                            entry.Property("DeletedAt").IsModified = false;
                            entry.Property("DeletedBy").IsModified = false;

                            intAuditEntity.UpdatedAt = DateTime.UtcNow;
                            intAuditEntity.UpdatedBy = currentUserId;
                            break;

                        case EntityState.Deleted:
                            entry.State = EntityState.Modified;
                            intAuditEntity.DeletedAt = DateTime.UtcNow;
                            intAuditEntity.DeletedBy = currentUserId;
                            break;
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // =========================================================================
            // 1. AUTH MODULE CONFIGURATION
            // =========================================================================
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(r => r.RoleName).IsUnique();
                entity.Property(r => r.RoleName).HasMaxLength(50).IsRequired();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Username).HasMaxLength(50).IsRequired();
                entity.Property(u => u.Email).HasMaxLength(100).IsRequired();
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });
                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade); // Cascade mapping is fine for join table delete
                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasQueryFilter(ur => ur.User.DeletedAt == null && ur.Role.DeletedAt == null);
            });

            // =========================================================================
            // 2. MASTER DATA CONFIGURATION
            // =========================================================================
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasIndex(c => c.Phone).IsUnique();
                entity.Property(c => c.CustomerName).HasMaxLength(100).IsRequired();
                entity.Property(c => c.Phone).HasMaxLength(20).IsRequired();
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasIndex(s => s.Phone).IsUnique();
                entity.Property(s => s.SupplierName).HasMaxLength(100).IsRequired();
                entity.Property(s => s.Phone).HasMaxLength(20).IsRequired();
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.CategoryName).IsUnique();
                entity.Property(c => c.CategoryName).HasMaxLength(100).IsRequired();
                entity.HasOne(c => c.ParentCategory)
                    .WithMany(c => c.SubCategories)
                    .HasForeignKey(c => c.ParentId)
                    .OnDelete(DeleteBehavior.Restrict); // FIXED: No Cascade Delete
            });

            modelBuilder.Entity<Unit>(entity =>
            {
                entity.HasIndex(u => u.UnitName).IsUnique(); 
                entity.Property(u => u.UnitName).HasMaxLength(50).IsRequired();
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => p.Sku).IsUnique();
                entity.Property(p => p.Sku).HasMaxLength(50).IsRequired();
                entity.Property(p => p.ProductName).HasMaxLength(150).IsRequired();
                
                entity.ToTable(t => t.HasCheckConstraint("ck_product_cost_price", "cost_price >= 0"));
                entity.ToTable(t => t.HasCheckConstraint("ck_product_selling_price", "selling_price >= 0"));

                // Protect relationships from dropping products
                entity.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Unit).WithMany(u => u.Products).HasForeignKey(p => p.UnitId).OnDelete(DeleteBehavior.Restrict);
            });

            // =========================================================================
            // 3. INVENTORY & TRANSACTIONS CONFIGURATION
            // =========================================================================
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.Property<uint>("xmin")
                    .HasColumnName("xmin")
                    .IsRowVersion();

                entity.HasOne(i => i.Product)
                    .WithOne(p => p.Inventory)
                    .HasForeignKey<Inventory>(i => i.ProductId)
                    .OnDelete(DeleteBehavior.Restrict); // FIXED: Protect product links

                entity.ToTable(t => t.HasCheckConstraint("ck_inventory_quantity", "quantity >= 0"));
            });

            modelBuilder.Entity<StockTransaction>(entity =>
            {
                entity.HasOne(st => st.Product)
                    .WithMany(p => p.StockTransactions)
                    .HasForeignKey(st => st.ProductId)
                    .OnDelete(DeleteBehavior.Restrict); // FIXED: Protect transaction log
            });

            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.HasOne(po => po.Creator).WithMany(u => u.CreatedPurchaseOrders).HasForeignKey(po => po.CreatedBy).OnDelete(DeleteBehavior.Restrict); // FIXED
                entity.HasOne(po => po.Supplier).WithMany(s => s.PurchaseOrders).HasForeignKey(po => po.SupplierId).OnDelete(DeleteBehavior.Restrict); // FIXED
            });

            modelBuilder.Entity<PurchaseOrderItem>(entity =>
            {
                entity.HasOne(poi => poi.PurchaseOrder).WithMany(o => o.Items).HasForeignKey(poi => poi.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade); // Cascade items when order is deleted
                entity.HasOne(poi => poi.Product).WithMany().HasForeignKey(poi => poi.ProductId).OnDelete(DeleteBehavior.Restrict); // FIXED
                
                entity.ToTable(t => t.HasCheckConstraint("ck_po_item_quantity", "quantity > 0"));
                entity.ToTable(t => t.HasCheckConstraint("ck_po_item_unit_price", "unit_price >= 0"));
            });

            modelBuilder.Entity<SalesOrder>(entity =>
            {
                entity.HasOne(so => so.Creator).WithMany(u => u.CreatedSalesOrders).HasForeignKey(so => so.CreatedBy).OnDelete(DeleteBehavior.Restrict); // FIXED
                entity.HasOne(so => so.Customer).WithMany(c => c.SalesOrders).HasForeignKey(so => so.CustomerId).OnDelete(DeleteBehavior.Restrict); // FIXED
            });

            modelBuilder.Entity<SalesOrderItem>(entity =>
            {
                entity.HasOne(soi => soi.SalesOrder).WithMany(o => o.Items).HasForeignKey(soi => soi.SalesOrderId).OnDelete(DeleteBehavior.Cascade); // Cascade items when order is deleted
                entity.HasOne(soi => soi.Product).WithMany().HasForeignKey(soi => soi.ProductId).OnDelete(DeleteBehavior.Restrict); // FIXED
                
                entity.ToTable(t => t.HasCheckConstraint("ck_so_item_quantity", "quantity > 0"));
                entity.ToTable(t => t.HasCheckConstraint("ck_so_item_unit_price", "unit_price >= 0"));
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasOne(p => p.SalesOrder).WithMany(o => o.Payments).HasForeignKey(p => p.SalesOrderId).OnDelete(DeleteBehavior.Restrict); // FIXED: Protect payment tracking
                
                entity.ToTable(t => t.HasCheckConstraint("ck_payment_amount", "payment_amount > 0"));
            });

            // =========================================================================
            // 4. CONVENTIONS: SNAKE_CASE NAMING, DEFAULT UUIDS, & SOFT DELETE FILTERS
            // =========================================================================
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Convert table name to lowercase snake_case
                var tableName = entity.GetTableName();
                if (tableName != null)
                {
                    entity.SetTableName(ToSnakeCase(tableName));
                }

                // Convert columns, primary keys, foreign keys, indexes to snake_case
                foreach (var property in entity.GetProperties())
                {
                    var storeObjectIdentifier = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
                    var columnName = property.GetColumnName(storeObjectIdentifier);
                    if (columnName != null)
                    {
                        property.SetColumnName(ToSnakeCase(columnName));
                    }

                    // Standardize UUID generation on add using native PostgreSQL gen_random_uuid()
                    if (property.ClrType == typeof(Guid) && property.IsPrimaryKey())
                    {
                        property.SetDefaultValueSql("gen_random_uuid()");
                    }

                    // Standardize CreatedAt, PaymentDate & LastUpdated default value on add
                    if ((property.Name == "CreatedAt" || property.Name == "PaymentDate" || property.Name == "LastUpdated") && property.ClrType == typeof(DateTime))
                    {
                        property.SetDefaultValueSql("CURRENT_TIMESTAMP");
                    }
                }

                foreach (var key in entity.GetKeys())
                {
                    var keyName = key.GetName();
                    if (keyName != null) key.SetName(ToSnakeCase(keyName));
                }

                foreach (var fk in entity.GetForeignKeys())
                {
                    var fkName = fk.GetConstraintName();
                    if (fkName != null) fk.SetConstraintName(ToSnakeCase(fkName));
                }

                foreach (var index in entity.GetIndexes())
                {
                    var indexName = index.GetDatabaseName();
                    if (indexName != null) index.SetDatabaseName(ToSnakeCase(indexName));
                }

                // Configure Global Soft Delete Filter
                var deletedAtProp = entity.FindProperty("DeletedAt");
                if (deletedAtProp != null && deletedAtProp.ClrType == typeof(DateTime?))
                {
                    var parameter = Expression.Parameter(entity.ClrType, "e");
                    var propertyAccess = Expression.Property(parameter, "DeletedAt");
                    var nullConstant = Expression.Constant(null, typeof(DateTime?));
                    var equal = Expression.Equal(propertyAccess, nullConstant);
                    var lambda = Expression.Lambda(equal, parameter);

                    modelBuilder.Entity(entity.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        private static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            
            var result = Regex.Replace(input, "([a-z0-9])([A-Z])", "$1_$2");
            return result.ToLower();
        }
    }
}
