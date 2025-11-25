using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetBrightener.Core.Logging.PostgreSqlDbStorage.Migrations
{
    /// <inheritdoc />
    public partial class Add_Id_Column_To_EventLog_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                schema: "Log",
                table: "EventLog",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventLog",
                schema: "Log",
                table: "EventLog",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
