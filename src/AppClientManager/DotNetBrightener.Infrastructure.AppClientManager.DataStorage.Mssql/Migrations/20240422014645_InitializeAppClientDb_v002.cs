using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DotNetBrightener.Infrastructure.AppClientManager.DataStorage.Mssql.Migrations
{
    /// <inheritdoc />
    public partial class InitializeAppClientDb_v002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "AppClient",
                table: "AppClientTypeLookup",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.UpdateData(
                schema: "AppClient",
                table: "AppClientTypeLookup",
                keyColumn: "Id",
                keyValue: 0,
                column: "AppClientTypeValue",
                value: "NoRestriction");

            migrationBuilder.InsertData(
                schema: "AppClient",
                table: "AppClientTypeLookup",
                columns: new[] { "Id", "AppClientTypeValue" },
                values: new object[,]
                {
                    { 5, "Web" },
                    { 20, "Desktop" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "AppClient",
                table: "AppClientTypeLookup",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "AppClient",
                table: "AppClientTypeLookup",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.UpdateData(
                schema: "AppClient",
                table: "AppClientTypeLookup",
                keyColumn: "Id",
                keyValue: 0,
                column: "AppClientTypeValue",
                value: "Web");

            migrationBuilder.InsertData(
                schema: "AppClient",
                table: "AppClientTypeLookup",
                columns: new[] { "Id", "AppClientTypeValue" },
                values: new object[] { 50, "Desktop" });
        }
    }
}
