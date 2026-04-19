using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Voluntrio.Common.Persistence.Context;

#nullable disable

namespace Voluntrio.Organizations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260416193000_AddIdentityUsersV1")]
    public partial class AddIdentityUsersV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    given_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    family_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                schema: "identity",
                table: "users",
                column: "email");

            migrationBuilder.Sql(
                """
                INSERT INTO identity.users (id, email, given_name, family_name, created_at_utc, updated_at_utc)
                SELECT DISTINCT ei.internal_user_id, ei.email, ei.given_name, ei.family_name, ei.updated_at_utc, ei.updated_at_utc
                FROM identity.external_identities ei
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM identity.users u
                    WHERE u.id = ei.internal_user_id
                );
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_external_identities_users_internal_user_id",
                schema: "identity",
                table: "external_identities",
                column: "internal_user_id",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_external_identities_users_internal_user_id",
                schema: "identity",
                table: "external_identities");

            migrationBuilder.DropTable(
                name: "users",
                schema: "identity");
        }
    }
}
