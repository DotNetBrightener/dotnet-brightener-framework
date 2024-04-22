using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DotNetBrightener.Infrastructure.AppClientManager.DataStorage.Mssql.Migrations
{
    /// <inheritdoc />
    public partial class InitializeAppClientDb_v001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AppClient");

            migrationBuilder.CreateTable(
                name: "AppClient",
                schema: "AppClient",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ClientType = table.Column<int>(type: "int", nullable: false),
                    ClientStatus = table.Column<int>(type: "int", nullable: false),
                    ClientSecretHashed = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    ClientSecretSalt = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AllowedOrigins = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AllowedAppBundleIds = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DeactivatedReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DeletionReason = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppClient", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppClientStatusLookup",
                schema: "AppClient",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    AppClientStatusValue = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppClientStatusLookup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppClientTypeLookup",
                schema: "AppClient",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    AppClientTypeValue = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppClientTypeLookup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppClientAccessScope",
                schema: "AppClient",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppClientId = table.Column<long>(type: "bigint", nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppClientAccessScope", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppClientAccessScope_AppClient_AppClientId",
                        column: x => x.AppClientId,
                        principalSchema: "AppClient",
                        principalTable: "AppClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "AppClient",
                table: "AppClientStatusLookup",
                columns: new[] { "Id", "AppClientStatusValue" },
                values: new object[,]
                {
                    { 0, "Inactive" },
                    { 30, "Active" },
                    { 50, "Suspended" }
                });

            migrationBuilder.InsertData(
                schema: "AppClient",
                table: "AppClientTypeLookup",
                columns: new[] { "Id", "AppClientTypeValue" },
                values: new object[,]
                {
                    { 0, "Web" },
                    { 10, "Mobile" },
                    { 50, "Desktop" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppClient_AllowedOrigins",
                schema: "AppClient",
                table: "AppClient",
                column: "AllowedOrigins");

            migrationBuilder.CreateIndex(
                name: "IX_AppClient_ClientId",
                schema: "AppClient",
                table: "AppClient",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppClientAccessScope_AppClientId",
                schema: "AppClient",
                table: "AppClientAccessScope",
                column: "AppClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppClientAccessScope",
                schema: "AppClient");

            migrationBuilder.DropTable(
                name: "AppClientStatusLookup",
                schema: "AppClient");

            migrationBuilder.DropTable(
                name: "AppClientTypeLookup",
                schema: "AppClient");

            migrationBuilder.DropTable(
                name: "AppClient",
                schema: "AppClient");
        }
    }
}
