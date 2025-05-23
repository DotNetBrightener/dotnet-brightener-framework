﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetBrightener.Core.Logging.DbStorage.Migrations
{
    /// <inheritdoc />
    public partial class InitLogTable : Migration
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

            migrationBuilder.Sql($@"
CREATE TABLE [Log].[EventLog](
	        [Id] [BIGINT] IDENTITY(1,1) NOT NULL,
	        [LoggerName] [NVARCHAR](1024) NULL,
	        [Level] [NVARCHAR](32) NULL,
	        [FormattedMessage] [NVARCHAR](MAX) NULL,
	        [Message] [NVARCHAR](MAX) NULL,
	        [TimeStamp] [DATETIME2](7) NOT NULL,
	        [RequestUrl] [NVARCHAR](MAX) NULL,
	        [RemoteIpAddress] [NVARCHAR](64) NULL,
	        [RequestId] [NVARCHAR](512) NULL,
	        [UserAgent] [NVARCHAR](MAX) NULL,
	        [TenantIds] [NVARCHAR](MAX) NULL,
	        [FullMessage] [NVARCHAR](MAX) NULL,
	        [StackTrace] [NVARCHAR](MAX) NULL,
	        [PropertiesDictionary] [NVARCHAR](MAX) NULL,
 CONSTRAINT [PK_EventLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

");

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
