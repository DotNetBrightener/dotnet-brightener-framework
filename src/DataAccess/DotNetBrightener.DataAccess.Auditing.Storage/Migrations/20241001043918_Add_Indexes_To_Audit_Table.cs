using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetBrightener.DataAccess.Auditing.Storage.Migrations
{
    /// <inheritdoc />
    public partial class Add_Indexes_To_Audit_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AuditEntity_EntityType_EntityIdentifier",
                schema: "Auditing",
                table: "AuditEntity",
                columns: new[] { "EntityType", "EntityIdentifier" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntity_StartTime",
                schema: "Auditing",
                table: "AuditEntity",
                column: "StartTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditEntity_EntityType_EntityIdentifier",
                schema: "Auditing",
                table: "AuditEntity");

            migrationBuilder.DropIndex(
                name: "IX_AuditEntity_StartTime",
                schema: "Auditing",
                table: "AuditEntity");
        }
    }
}
