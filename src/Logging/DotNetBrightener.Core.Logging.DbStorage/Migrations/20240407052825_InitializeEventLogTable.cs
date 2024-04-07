using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetBrightener.Core.Logging.DbStorage.Migrations
{
    /// <inheritdoc />
    public partial class InitializeEventLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Logging");

            migrationBuilder.CreateTable(
                name: "EventLog",
                schema: "Logging",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoggerName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Level = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    FormattedMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RemoteIpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    RequestId = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StackTrace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PropertiesDictionary = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLog", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_LoggerName",
                schema: "Logging",
                table: "EventLog",
                column: "LoggerName")
                .Annotation("SqlServer:Include", new[] { "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_RequestId",
                schema: "Logging",
                table: "EventLog",
                column: "RequestId")
                .Annotation("SqlServer:Include", new[] { "TimeStamp", "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_TimeStamp",
                schema: "Logging",
                table: "EventLog",
                column: "TimeStamp")
                .Annotation("SqlServer:Include", new[] { "Level" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventLog",
                schema: "Logging");
        }
    }
}
