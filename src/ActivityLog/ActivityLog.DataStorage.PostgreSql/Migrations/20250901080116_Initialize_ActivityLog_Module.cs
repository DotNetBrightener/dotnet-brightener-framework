using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActivityLog.DataStorage.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class Initialize_ActivityLog_Module : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ActivityLog");

            migrationBuilder.CreateTable(
                name: "ActivityLogRecord",
                schema: "ActivityLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ActivityDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TargetEntity = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TargetEntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExecutionDurationMs = table.Column<double>(type: "double precision", nullable: true),
                    MethodName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ClassName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Namespace = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    InputParameters = table.Column<string>(type: "text", nullable: true),
                    ReturnValue = table.Column<string>(type: "text", nullable: true),
                    Exception = table.Column<string>(type: "text", nullable: true),
                    ExceptionType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    Metadata = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
                    LogLevel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Tags = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogRecord", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLog_ClassName",
                schema: "ActivityLog",
                table: "ActivityLogRecord",
                column: "ClassName");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLog_CorrelationId",
                schema: "ActivityLog",
                table: "ActivityLogRecord",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLog_IsSuccess",
                schema: "ActivityLog",
                table: "ActivityLogRecord",
                column: "IsSuccess");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLog_LogLevel",
                schema: "ActivityLog",
                table: "ActivityLogRecord",
                column: "LogLevel");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLog_MethodName",
                schema: "ActivityLog",
                table: "ActivityLogRecord",
                column: "MethodName");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLog_Namespace",
                schema: "ActivityLog",
                table: "ActivityLogRecord",
                column: "Namespace");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLog_Namespace_ClassName_StartTime",
                schema: "ActivityLog",
                table: "ActivityLogRecord",
                columns: new[] { "Namespace", "ClassName", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLog_StartTime",
                schema: "ActivityLog",
                table: "ActivityLogRecord",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLog_StartTime_IsSuccess",
                schema: "ActivityLog",
                table: "ActivityLogRecord",
                columns: new[] { "StartTime", "IsSuccess" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLog_UserId",
                schema: "ActivityLog",
                table: "ActivityLogRecord",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogRecord",
                schema: "ActivityLog");
        }
    }
}
