using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voluntrio.Organizations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationsHierarchyV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "parent_organization_id",
                schema: "org",
                table: "organizations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_organizations_parent_organization_id",
                schema: "org",
                table: "organizations",
                column: "parent_organization_id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_organizations_parent_not_self",
                schema: "org",
                table: "organizations",
                sql: "parent_organization_id IS NULL OR parent_organization_id <> id");

            migrationBuilder.AddForeignKey(
                name: "FK_organizations_organizations_parent_organization_id",
                schema: "org",
                table: "organizations",
                column: "parent_organization_id",
                principalSchema: "org",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_organizations_organizations_parent_organization_id",
                schema: "org",
                table: "organizations");

            migrationBuilder.DropIndex(
                name: "IX_organizations_parent_organization_id",
                schema: "org",
                table: "organizations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_organizations_parent_not_self",
                schema: "org",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "parent_organization_id",
                schema: "org",
                table: "organizations");
        }
    }
}
