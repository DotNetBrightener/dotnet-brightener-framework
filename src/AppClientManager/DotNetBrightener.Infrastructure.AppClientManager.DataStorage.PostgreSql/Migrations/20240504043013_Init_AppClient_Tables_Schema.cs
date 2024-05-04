using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DotNetBrightener.Infrastructure.AppClientManager.DataStorage.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class Init_AppClient_Tables_Schema : Migration
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
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ClientType = table.Column<int>(type: "integer", nullable: false),
                    ClientStatus = table.Column<int>(type: "integer", nullable: false),
                    ClientSecretHashed = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    ClientSecretSalt = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AllowedOrigins = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AllowedAppBundleIds = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DeactivatedReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DeletionReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false),
                    AppClientStatusValue = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false),
                    AppClientTypeValue = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
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
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppClientId = table.Column<long>(type: "bigint", nullable: false),
                    Scope = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Destination = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
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
                    { 0, "NoRestriction" },
                    { 5, "Web" },
                    { 10, "Mobile" },
                    { 20, "Desktop" }
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
