using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voluntrio.Organizations.Infrastructure.Persistence.Migrations
{
    public partial class InitialOrganizations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "common");
            migrationBuilder.EnsureSchema(name: "org");

            migrationBuilder.CreateTable(
                name: "idempotency_keys",
                schema: "common",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    processed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idempotency_keys", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "common",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    received_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    type = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    processed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organizations",
                schema: "org",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "common",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    kind = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    type = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organization_memberships",
                schema: "org",
                columns: table => new
                {
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    joined_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    left_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_memberships", x => new { x.organization_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_organization_memberships_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "org",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inbox_messages_processed_at_utc",
                schema: "common",
                table: "inbox_messages",
                column: "processed_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_organization_memberships_organization_id",
                schema: "org",
                table: "organization_memberships",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_memberships_status",
                schema: "org",
                table: "organization_memberships",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_organization_memberships_user_id",
                schema: "org",
                table: "organization_memberships",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_slug",
                schema: "org",
                table: "organizations",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_processed_on_utc",
                schema: "common",
                table: "outbox_messages",
                column: "processed_on_utc");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "idempotency_keys", schema: "common");
            migrationBuilder.DropTable(name: "inbox_messages", schema: "common");
            migrationBuilder.DropTable(name: "organization_memberships", schema: "org");
            migrationBuilder.DropTable(name: "outbox_messages", schema: "common");
            migrationBuilder.DropTable(name: "organizations", schema: "org");
        }
    }
}
