using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangePhoneIndexToPrefix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_customers_phone_trgm",
                table: "customers");

            migrationBuilder.CreateIndex(
                name: "ix_customers_phone_prefix",
                table: "customers",
                column: "phone",
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexOperators", new[] { "varchar_pattern_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_customers_phone_prefix",
                table: "customers");

            migrationBuilder.CreateIndex(
                name: "ix_customers_phone_trgm",
                table: "customers",
                column: "phone",
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }
    }
}
