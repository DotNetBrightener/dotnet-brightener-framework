using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetBrightener.TemplateEngine.Data.Mssql.Migrations
{
    /// <inheritdoc />
    public partial class InitializeTemplateEngineSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "TemplateEngine");

            migrationBuilder.Sql($@"
DECLARE @tableSchema NVARCHAR(128) = 'TemplateEngine';
DECLARE @tableName NVARCHAR(128) = 'TemplateRecord';

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
                name: "TemplateRecord",
                schema: "TemplateEngine",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateType = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    TemplateTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromAssemblyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DeletionReason = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateRecord", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateRecord_TemplateType",
                schema: "TemplateEngine",
                table: "TemplateRecord",
                column: "TemplateType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TemplateRecord",
                schema: "TemplateEngine");
        }
    }
}
