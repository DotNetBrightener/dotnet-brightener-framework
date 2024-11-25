using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetBrightener.DataAccess.Auditing.Storage.Migrations
{
    /// <inheritdoc />
    public partial class Resize_EntityIdentifier_And_Drop_FullName_Column : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditEntity_EntityType_EntityIdentifier",
                schema: "Auditing",
                table: "AuditEntity");

            migrationBuilder.DropColumn(
                name: "EntityTypeFullName",
                schema: "Auditing",
                table: "AuditEntity");

            migrationBuilder.AlterColumn<string>(
                name: "EntityIdentifier",
                schema: "Auditing",
                table: "AuditEntity",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntity_EntityType",
                schema: "Auditing",
                table: "AuditEntity",
                column: "EntityType")
                .Annotation("SqlServer:Include", new[] { "StartTime", "Action" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditEntity_EntityType",
                schema: "Auditing",
                table: "AuditEntity");

            migrationBuilder.AlterColumn<string>(
                name: "EntityIdentifier",
                schema: "Auditing",
                table: "AuditEntity",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityTypeFullName",
                schema: "Auditing",
                table: "AuditEntity",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntity_EntityType_EntityIdentifier",
                schema: "Auditing",
                table: "AuditEntity",
                columns: new[] { "EntityType", "EntityIdentifier" });
        }
    }
}
