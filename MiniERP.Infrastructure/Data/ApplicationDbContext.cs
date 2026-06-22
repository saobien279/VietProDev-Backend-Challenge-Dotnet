using Microsoft.EntityFrameworkCore;
using MiniERP.Domain.Entities;

namespace MiniERP.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình bảng User Roles (Nhóm Auth)
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });
                entity.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);
                entity.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Username).HasMaxLength(50);
                entity.Property(u => u.Email).HasMaxLength(100);
            });

            // 2. Cấu hình Đối tác (Customers & Suppliers)
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasIndex(c => c.Phone).IsUnique();
                entity.Property(c => c.CustomerName).HasMaxLength(100);
                entity.Property(c => c.Phone).HasMaxLength(20);
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasIndex(s => s.Phone).IsUnique();
                entity.Property(s => s.SupplierName).HasMaxLength(100);
                entity.Property(s => s.Phone).HasMaxLength(20);
            });

            // 3. Cấu hình Categories (Self-Referencing)
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasOne(c => c.ParentCategory)
                      .WithMany(c => c.SubCategories)
                      .HasForeignKey(c => c.ParentId)
                      .OnDelete(DeleteBehavior.Restrict); // Tránh xóa cha làm ảnh hưởng dây chuyền lỗi
            });

            // 4. Cấu hình Products & Inventory
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => p.Sku).IsUnique();
                entity.Property(p => p.Sku).HasMaxLength(50);
                entity.Property(p => p.ProductName).HasMaxLength(150);
                
                // Ràng buộc giá >= 0 trong PostgreSQL
                entity.ToTable(t => t.HasCheckConstraint("CK_Product_CostPrice", "\"CostPrice\" >= 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Product_SellingPrice", "\"SellingPrice\" >= 0"));
            });

            modelBuilder.Entity<Inventory>(entity =>
            {
                // Quan hệ 1 - 1 giữa Product và Inventory
                entity.HasOne(i => i.Product)
                      .WithOne(p => p.Inventory)
                      .HasForeignKey<Inventory>(i => i.ProductId);

                entity.ToTable(t => t.HasCheckConstraint("CK_Inventory_Quantity", "\"Quantity\" >= 0"));
            });

            // 5. Cấu hình Transactions (Đơn mua / Đơn bán / Thanh toán)
            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.HasOne(po => po.Creator).WithMany(u => u.CreatedPurchaseOrders).HasForeignKey(po => po.CreatedBy);
            });

            modelBuilder.Entity<SalesOrder>(entity =>
            {
                entity.HasOne(so => so.Creator).WithMany(u => u.CreatedSalesOrders).HasForeignKey(so => so.CreatedBy);
            });

            modelBuilder.Entity<PurchaseOrderItem>(entity =>
            {
                entity.ToTable(t => t.HasCheckConstraint("CK_PO_Item_Quantity", "\"Quantity\" > 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_PO_Item_UnitPrice", "\"UnitPrice\" >= 0"));
            });

            modelBuilder.Entity<SalesOrderItem>(entity =>
            {
                entity.ToTable(t => t.HasCheckConstraint("CK_SO_Item_Quantity", "\"Quantity\" > 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_SO_Item_UnitPrice", "\"UnitPrice\" >= 0"));
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable(t => t.HasCheckConstraint("CK_Payment_Amount", "\"PaymentAmount\" > 0"));
            });
        }
    }
}
