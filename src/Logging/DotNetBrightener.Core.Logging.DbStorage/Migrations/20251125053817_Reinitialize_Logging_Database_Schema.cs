using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetBrightener.Core.Logging.DbStorage.Migrations
{
    /// <inheritdoc />
    public partial class Reinitialize_Logging_Database_Schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Log");

            migrationBuilder.Sql($@"
DECLARE @tableSchema NVARCHAR(128) = 'Log';
DECLARE @tableName NVARCHAR(128) = 'EventLog';

IF EXISTS (SELECT * FROM information_schema.tables WHERE table_schema = @tableSchema AND table_name = @tableName)
BEGIN    
    DECLARE @newTableName NVARCHAR(128);
    SET @newTableName = @tableName + '_Old_' + REPLACE(REPLACE(REPLACE(CONVERT(NVARCHAR, GETDATE(), 120), '-', ''), ' ', '_'), ':', '');

    DECLARE @pkName NVARCHAR(128);
    
    SELECT @pkName = constraint_name 
    FROM information_schema.table_constraints 
    WHERE constraint_type = 'PRIMARY KEY' AND table_schema = @tableSchema AND table_name = @tableName;
    
    IF @pkName IS NOT NULL
    BEGIN
        DECLARE @dropPKStatement NVARCHAR(MAX);
        SET @dropPKStatement = 'ALTER TABLE ' + @tableSchema + '.' + @tableName + ' DROP CONSTRAINT ' + @pkName;

        EXEC sp_executesql @dropPKStatement;
    END

    DECLARE @currentTableName NVARCHAR(128) = @tableSchema + '.' + @tableName;
    EXEC sp_rename @currentTableName, @newTableName;
END
");

            migrationBuilder.CreateTable(
                name: "EventLog",
                schema: "Log",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoggerName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Level = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FormattedMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RemoteIpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestId = table.Column<string>(type: "nvarchar(450)", nullable: true),
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
                name: "IX_EventLog_Level",
                schema: "Log",
                table: "EventLog",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_LoggerName",
                schema: "Log",
                table: "EventLog",
                column: "LoggerName")
                .Annotation("SqlServer:Include", new[] { "Level", "TimeStamp" });

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_RequestId",
                schema: "Log",
                table: "EventLog",
                column: "RequestId")
                .Annotation("SqlServer:Include", new[] { "TimeStamp", "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_TimeStamp",
                schema: "Log",
                table: "EventLog",
                column: "TimeStamp")
                .Annotation("SqlServer:Include", new[] { "Level", "LoggerName" });
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
