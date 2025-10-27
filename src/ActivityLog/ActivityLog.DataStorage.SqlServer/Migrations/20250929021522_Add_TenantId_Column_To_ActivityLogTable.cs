using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActivityLog.DataStorage.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class Add_TenantId_Column_To_ActivityLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "ActivityLog",
                table: "ActivityLogRecord",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "ActivityLog",
                table: "ActivityLogRecord");
        }
    }
}
