using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GC.Account.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAlias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Alias",
                table: "Accounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Alias",
                table: "Accounts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
