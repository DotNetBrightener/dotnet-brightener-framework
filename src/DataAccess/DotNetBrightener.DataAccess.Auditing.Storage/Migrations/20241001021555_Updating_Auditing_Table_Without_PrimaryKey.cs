using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetBrightener.DataAccess.Auditing.Storage.Migrations
{
    /// <inheritdoc />
    public partial class Updating_Auditing_Table_Without_PrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
