using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Voluntrio.Common.Persistence.Context;

#nullable disable

namespace Voluntrio.Organizations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260416203000_AddAvatarUrlToIdentityUsers")]
    public partial class AddAvatarUrlToIdentityUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "avatar_url",
                schema: "identity",
                table: "users",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE identity.users
                SET avatar_url = 'https://www.gravatar.com/avatar/' || md5(lower(btrim(email))) || '?d=identicon'
                WHERE avatar_url IS NULL
                  AND email IS NOT NULL
                  AND btrim(email) <> '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "avatar_url",
                schema: "identity",
                table: "users");
        }
    }
}
