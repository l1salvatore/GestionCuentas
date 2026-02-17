using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GC.Account.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueUserIdToAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserId",
                table: "Accounts",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_UserId",
                table: "Accounts");
        }
    }
}
