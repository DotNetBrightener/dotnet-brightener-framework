using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetBrightener.DataAccess.Auditing.Storage.Migrations
{
    /// <inheritdoc />
    public partial class Initialize_Auditing_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Auditing");

            migrationBuilder.CreateTable(
                name: "AuditEntity",
                schema: "Auditing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Changes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EntityIdentifier = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    EntityTypeFullName = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EndTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: true),
                    ScopeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntity", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditEntity",
                schema: "Auditing");
        }
    }
}
