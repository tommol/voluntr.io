using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voluntrio.Organizations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityMappingsV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.CreateTable(
                name: "external_identities",
                schema: "identity",
                columns: table => new
                {
                    provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    external_subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    internal_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    given_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    family_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_identities", x => new { x.provider, x.external_subject });
                });

            migrationBuilder.CreateIndex(
                name: "IX_external_identities_internal_user_id",
                schema: "identity",
                table: "external_identities",
                column: "internal_user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "external_identities",
                schema: "identity");
        }
    }
}
