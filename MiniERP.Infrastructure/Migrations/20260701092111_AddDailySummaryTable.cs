using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDailySummaryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_summaries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    summary_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total_sales_revenue = table.Column<decimal>(type: "numeric", nullable: false),
                    total_sales_orders_count = table.Column<int>(type: "integer", nullable: false),
                    total_purchase_cost = table.Column<decimal>(type: "numeric", nullable: false),
                    total_purchase_orders_count = table.Column<int>(type: "integer", nullable: false),
                    low_stock_products_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_daily_summaries", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_daily_summaries_summary_date",
                table: "daily_summaries",
                column: "summary_date",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_summaries");
        }
    }
}
