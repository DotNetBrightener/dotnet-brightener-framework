using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationService.Persistent.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class Initialize_NotificationService_Module_Database : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "NotificationService");

            migrationBuilder.CreateTable(
                name: "NotificationMessageQueue",
                schema: "NotificationService",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationTypeId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DeliveryTarget = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    CcTargets = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    BccTargets = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    MessageBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MessageTitle = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    SenderEntityId = table.Column<long>(type: "bigint", nullable: false),
                    NeedSendImmediately = table.Column<bool>(type: "bit", nullable: false),
                    EnqueuedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PlanToSendAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SentAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastAttemptUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastAttemptException = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationMessageQueue", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationMessageQueue",
                schema: "NotificationService");
        }
    }
}
