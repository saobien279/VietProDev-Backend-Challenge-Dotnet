using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerEmailPrefixIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_customers_email_prefix",
                table: "customers",
                column: "email",
                filter: "deleted_at IS NULL AND email IS NOT NULL")
                .Annotation("Npgsql:IndexOperators", new[] { "varchar_pattern_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_customers_email_prefix",
                table: "customers");
        }
    }
}
