using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DotNetBrightener.Integration.DataMigration.Migrations.DataMigrationPostgreSQLDb
{
    public partial class InitializeDataMigrationSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "__DataMigrationHistories",
                columns: table => new
                {
                    MigrationId = table.Column<string>(type: "text", nullable: false),
                    ModuleId = table.Column<string>(type: "text", nullable: true),
                    MigrationRecorded = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK___DataMigrationHistories", x => x.MigrationId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "__DataMigrationHistories");
        }
    }
}
