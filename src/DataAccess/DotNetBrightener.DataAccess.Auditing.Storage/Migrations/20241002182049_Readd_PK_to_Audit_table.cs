using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetBrightener.DataAccess.Auditing.Storage.Migrations
{
    /// <inheritdoc />
    public partial class Readd_PK_to_Audit_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ensure no duplicate records
            migrationBuilder.Sql("""
                                 WITH cte AS (
                                   SELECT Id, row_number() OVER(PARTITION BY Id ORDER BY StartTime) AS [rn]
                                   FROM Auditing.AuditEntity
                                 )
                                 DELETE cte WHERE [rn] > 1
                                 """);

            migrationBuilder.DropIndex(
                name: "IX_AuditEntity_Id",
                schema: "Auditing",
                table: "AuditEntity");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuditEntity",
                schema: "Auditing",
                table: "AuditEntity",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AuditEntity",
                schema: "Auditing",
                table: "AuditEntity");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntity_Id",
                schema: "Auditing",
                table: "AuditEntity",
                column: "Id");
        }
    }
}
