using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OC.LUAC.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddCountryNameToShippingZoneCountry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CountryCode",
                table: "ShippingZoneCountries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "CountryName",
                table: "ShippingZoneCountries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountryName",
                table: "ShippingZoneCountries");

            migrationBuilder.AlterColumn<string>(
                name: "CountryCode",
                table: "ShippingZoneCountries",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
