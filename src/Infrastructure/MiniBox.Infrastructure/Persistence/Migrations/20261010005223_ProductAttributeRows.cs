using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniBox.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProductAttributeRows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "ProductStocks",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "ProductAttributes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductAttributes_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'Products' AND column_name = 'Attributes') THEN
                        INSERT INTO "ProductAttributes" ("Id", "ProductId", "Key", "Value", "CreatedAt")
                        SELECT gen_random_uuid(), p."Id", item.key, item.value, now()
                        FROM "Products" p
                        CROSS JOIN LATERAL jsonb_each_text(COALESCE(p."Attributes", '{}'::jsonb)) AS item(key, value);
                        DROP INDEX IF EXISTS "IX_Products_Attributes";
                        ALTER TABLE "Products" DROP COLUMN "Attributes";
                    END IF;
                END $$;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributes_ProductId_Key_Value",
                table: "ProductAttributes",
                columns: new[] { "ProductId", "Key", "Value" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductAttributes");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "ProductStocks");
        }
    }
}
