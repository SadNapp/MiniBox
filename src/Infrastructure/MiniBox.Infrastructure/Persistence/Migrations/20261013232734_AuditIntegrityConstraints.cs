using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniBox.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AuditIntegrityConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "Users",
                type: "text",
                nullable: true,
                computedColumnSql: "upper(btrim(\"Email\"))",
                stored: true);
            migrationBuilder.Sql("UPDATE \"Users\" SET \"Email\" = btrim(\"Email\");");
            migrationBuilder.Sql("DELETE FROM \"Users\" a USING \"Users\" b WHERE a.ctid > b.ctid AND upper(btrim(a.\"Email\")) = upper(btrim(b.\"Email\"));");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email_Normalized",
                table: "Users",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_Discount_Range",
                table: "Products",
                sql: "\"Discount\" >= 0 AND \"Discount\" <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_Price_NonNegative",
                table: "Products",
                sql: "\"Price\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_Quantity_Positive",
                table: "OrderItems",
                sql: "\"Quantity\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_UnitPrice_NonNegative",
                table: "OrderItems",
                sql: "\"UnitPrice\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email_Normalized",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_Discount_Range",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_Price_NonNegative",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_Quantity_Positive",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_UnitPrice_NonNegative",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "Users");

        }
    }
}
