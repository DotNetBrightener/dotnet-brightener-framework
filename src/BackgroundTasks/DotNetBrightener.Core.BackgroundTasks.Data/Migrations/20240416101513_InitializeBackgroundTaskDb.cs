using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetBrightener.Core.BackgroundTasks.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitializeBackgroundTaskDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "BackgroundTask");

            migrationBuilder.CreateTable(
                name: "BackgroundTaskDefinition",
                schema: "BackgroundTask",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskAssembly = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TaskTypeName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CronExpression = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Description = table.Column<bool>(type: "bit", maxLength: 1000, nullable: false),
                    TimeZoneIANA = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    LastRunUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextRunUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastRunError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastRunDuration = table.Column<TimeSpan>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackgroundTaskDefinition", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundTaskDefinition_TaskAssembly",
                schema: "BackgroundTask",
                table: "BackgroundTaskDefinition",
                column: "TaskAssembly")
                .Annotation("SqlServer:Include", new[] { "TaskTypeName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackgroundTaskDefinition",
                schema: "BackgroundTask");
        }
    }
}
