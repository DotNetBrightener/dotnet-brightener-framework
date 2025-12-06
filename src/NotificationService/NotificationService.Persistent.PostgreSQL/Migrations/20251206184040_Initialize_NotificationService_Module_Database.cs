using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NotificationService.Persistent.PostgreSQL.Migrations
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
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NotificationTypeId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DeliveryTarget = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    CcTargets = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    BccTargets = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    MessageBody = table.Column<string>(type: "text", nullable: true),
                    MessageTitle = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    SenderEntityId = table.Column<long>(type: "bigint", nullable: false),
                    NeedSendImmediately = table.Column<bool>(type: "boolean", nullable: false),
                    EnqueuedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PlanToSendAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SentAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastAttemptUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastAttemptException = table.Column<string>(type: "text", nullable: true),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
