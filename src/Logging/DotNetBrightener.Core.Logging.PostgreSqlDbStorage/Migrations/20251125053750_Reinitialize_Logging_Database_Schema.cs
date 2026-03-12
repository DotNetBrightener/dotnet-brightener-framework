using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DotNetBrightener.Core.Logging.PostgreSqlDbStorage.Migrations
{
    /// <inheritdoc />
    public partial class Reinitialize_Logging_Database_Schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Log");

            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS ""Log"".""EventLog"" CASCADE;
            ");

            migrationBuilder.CreateTable(
                name: "EventLog",
                schema: "Log",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoggerName = table.Column<string>(type: "text", nullable: true),
                    Level = table.Column<string>(type: "text", nullable: true),
                    FormattedMessage = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequestUrl = table.Column<string>(type: "text", nullable: true),
                    RemoteIpAddress = table.Column<string>(type: "text", nullable: true),
                    RequestId = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    TenantIds = table.Column<string>(type: "text", nullable: true),
                    FullMessage = table.Column<string>(type: "text", nullable: true),
                    StackTrace = table.Column<string>(type: "text", nullable: true),
                    PropertiesDictionary = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLog", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_Level",
                schema: "Log",
                table: "EventLog",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_LoggerName",
                schema: "Log",
                table: "EventLog",
                column: "LoggerName")
                .Annotation("Npgsql:IndexInclude", new[] { "Level", "TimeStamp" });

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_RequestId",
                schema: "Log",
                table: "EventLog",
                column: "RequestId")
                .Annotation("Npgsql:IndexInclude", new[] { "TimeStamp", "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_TimeStamp",
                schema: "Log",
                table: "EventLog",
                column: "TimeStamp")
                .Annotation("Npgsql:IndexInclude", new[] { "Level", "LoggerName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventLog",
                schema: "Log");
        }
    }
}
