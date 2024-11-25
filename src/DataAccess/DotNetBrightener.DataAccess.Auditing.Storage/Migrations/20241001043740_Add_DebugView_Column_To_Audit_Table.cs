using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetBrightener.DataAccess.Auditing.Storage.Migrations
{
    /// <inheritdoc />
    public partial class Add_DebugView_Column_To_Audit_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DebugView",
                schema: "Auditing",
                table: "AuditEntity",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DebugView",
                schema: "Auditing",
                table: "AuditEntity");
        }
    }
}
