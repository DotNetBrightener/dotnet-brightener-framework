using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetBrightener.DataAccess.DataMigration.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class Init_DataMigration_Schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "DataMigration");

            migrationBuilder.CreateTable(
                name: "__DataMigrationsHistory",
                schema: "DataMigration",
                columns: table => new
                {
                    MigrationId = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    AppliedDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK___DataMigrationsHistory", x => x.MigrationId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "__DataMigrationsHistory",
                schema: "DataMigration");
        }
    }
}
