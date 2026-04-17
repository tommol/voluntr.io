using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Voluntrio.Common.Persistence.Context;

#nullable disable

namespace Voluntrio.Organizations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260416200000_AddKindeUserIdToIdentityUsers")]
    public partial class AddKindeUserIdToIdentityUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "kinde_user_id",
                schema: "identity",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE identity.users u
                SET kinde_user_id = ei.external_subject
                FROM identity.external_identities ei
                WHERE ei.internal_user_id = u.id
                  AND ei.provider = 'kinde'
                  AND u.kinde_user_id IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_users_kinde_user_id",
                schema: "identity",
                table: "users",
                column: "kinde_user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_kinde_user_id",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "kinde_user_id",
                schema: "identity",
                table: "users");
        }
    }
}
