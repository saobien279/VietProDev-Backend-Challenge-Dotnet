using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFieldsToStockTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "transaction_type",
                table: "stock_transactions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "reference_type",
                table: "stock_transactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "stock_after",
                table: "stock_transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "stock_before",
                table: "stock_transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reference_type",
                table: "stock_transactions");

            migrationBuilder.DropColumn(
                name: "stock_after",
                table: "stock_transactions");

            migrationBuilder.DropColumn(
                name: "stock_before",
                table: "stock_transactions");

            migrationBuilder.AlterColumn<string>(
                name: "transaction_type",
                table: "stock_transactions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);
        }
    }
}
