using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchAndPaginationIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_products_category_id",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_customers_phone",
                table: "customers");

            migrationBuilder.CreateIndex(
                name: "ix_sales_orders_status_date_partial",
                table: "sales_orders",
                columns: new[] { "status", "created_at" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_orders_status_date_partial",
                table: "purchase_orders",
                columns: new[] { "status", "created_at" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_products_category_id_partial",
                table: "products",
                column: "category_id",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_products_name_partial",
                table: "products",
                column: "product_name",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_customers_name_partial",
                table: "customers",
                column: "customer_name",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_customers_phone_partial",
                table: "customers",
                column: "phone",
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_sales_orders_status_date_partial",
                table: "sales_orders");

            migrationBuilder.DropIndex(
                name: "ix_purchase_orders_status_date_partial",
                table: "purchase_orders");

            migrationBuilder.DropIndex(
                name: "ix_products_category_id_partial",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_products_name_partial",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_customers_name_partial",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "ix_customers_phone_partial",
                table: "customers");

            migrationBuilder.CreateIndex(
                name: "ix_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_customers_phone",
                table: "customers",
                column: "phone",
                unique: true);
        }
    }
}
