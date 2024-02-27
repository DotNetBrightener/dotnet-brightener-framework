using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRUDWebApiWithGeneratorDemo.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ProductCategoryId",
                table: "Product",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Product_ProductCategoryId",
                table: "Product",
                column: "ProductCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_ProductCategory_ProductCategoryId",
                table: "Product",
                column: "ProductCategoryId",
                principalTable: "ProductCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_ProductCategory_ProductCategoryId",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_ProductCategoryId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ProductCategoryId",
                table: "Product");
        }
    }
}
