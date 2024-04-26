using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetBrightener.Core.Logging.DbStorage.Migrations
{
    /// <inheritdoc />
    public partial class RefineIndexesForLoggingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EventLog_LoggerName",
                schema: "Logging",
                table: "EventLog");

            migrationBuilder.DropIndex(
                name: "IX_EventLog_TimeStamp",
                schema: "Logging",
                table: "EventLog");

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_Level",
                schema: "Logging",
                table: "EventLog",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_LoggerName",
                schema: "Logging",
                table: "EventLog",
                column: "LoggerName")
                .Annotation("SqlServer:Include", new[] { "Level", "TimeStamp" });

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_TimeStamp",
                schema: "Logging",
                table: "EventLog",
                column: "TimeStamp")
                .Annotation("SqlServer:Include", new[] { "Level", "LoggerName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EventLog_Level",
                schema: "Logging",
                table: "EventLog");

            migrationBuilder.DropIndex(
                name: "IX_EventLog_LoggerName",
                schema: "Logging",
                table: "EventLog");

            migrationBuilder.DropIndex(
                name: "IX_EventLog_TimeStamp",
                schema: "Logging",
                table: "EventLog");

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_LoggerName",
                schema: "Logging",
                table: "EventLog",
                column: "LoggerName")
                .Annotation("SqlServer:Include", new[] { "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_TimeStamp",
                schema: "Logging",
                table: "EventLog",
                column: "TimeStamp")
                .Annotation("SqlServer:Include", new[] { "Level" });
        }
    }
}
