using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OC.LUAC.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddIsGuestToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGuest",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGuest",
                table: "Customers");
        }
    }
}
