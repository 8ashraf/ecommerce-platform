using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shop.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMembers_UserId",
                schema: "identity",
                table: "OrganizationMembers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Users_UserId",
                schema: "identity",
                table: "Accounts",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationMembers_Organizations_OrgId",
                schema: "identity",
                table: "OrganizationMembers",
                column: "OrgId",
                principalSchema: "identity",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationMembers_Users_UserId",
                schema: "identity",
                table: "OrganizationMembers",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Users_UserId",
                schema: "identity",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationMembers_Organizations_OrgId",
                schema: "identity",
                table: "OrganizationMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationMembers_Users_UserId",
                schema: "identity",
                table: "OrganizationMembers");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationMembers_UserId",
                schema: "identity",
                table: "OrganizationMembers");
        }
    }
}
