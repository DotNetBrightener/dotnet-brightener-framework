using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetBrightener.Core.Logging.DbStorage.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Key_From_Log_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EventLog",
                schema: "Log",
                table: "EventLog");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "Log",
                table: "EventLog");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Id",
                schema: "Log",
                table: "EventLog",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventLog",
                schema: "Log",
                table: "EventLog",
                column: "Id");
        }
    }
}
