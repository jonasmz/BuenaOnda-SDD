using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuenaOnda.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVariation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_sellable_options_ProductId",
                table: "sellable_options");

            migrationBuilder.AddColumn<string>(
                name: "Signature",
                table: "sellable_options",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "option_values",
                columns: table => new
                {
                    CharacteristicId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellableOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    NormalizedValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_option_values", x => new { x.SellableOptionId, x.CharacteristicId });
                    table.ForeignKey(
                        name: "FK_option_values_sellable_options_SellableOptionId",
                        column: x => x.SellableOptionId,
                        principalTable: "sellable_options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "variation_characteristics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_variation_characteristics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_variation_characteristics_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sellable_options_ProductId_Signature",
                table: "sellable_options",
                columns: new[] { "ProductId", "Signature" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_variation_characteristics_ProductId_NormalizedName",
                table: "variation_characteristics",
                columns: new[] { "ProductId", "NormalizedName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "option_values");

            migrationBuilder.DropTable(
                name: "variation_characteristics");

            migrationBuilder.DropIndex(
                name: "IX_sellable_options_ProductId_Signature",
                table: "sellable_options");

            migrationBuilder.DropColumn(
                name: "Signature",
                table: "sellable_options");

            migrationBuilder.CreateIndex(
                name: "IX_sellable_options_ProductId",
                table: "sellable_options",
                column: "ProductId");
        }
    }
}
