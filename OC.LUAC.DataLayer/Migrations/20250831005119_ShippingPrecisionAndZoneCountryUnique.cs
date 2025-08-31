using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OC.LUAC.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class ShippingPrecisionAndZoneCountryUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShippingZoneCountries_ShippingZoneId",
                table: "ShippingZoneCountries");

            migrationBuilder.AlterColumn<decimal>(
                name: "Percentage",
                table: "Vouchers",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CountryCode",
                table: "ShippingZoneCountries",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingZoneCountries_ShippingZoneId_CountryCode",
                table: "ShippingZoneCountries",
                columns: new[] { "ShippingZoneId", "CountryCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShippingZoneCountries_ShippingZoneId_CountryCode",
                table: "ShippingZoneCountries");

            migrationBuilder.AlterColumn<decimal>(
                name: "Percentage",
                table: "Vouchers",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CountryCode",
                table: "ShippingZoneCountries",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingZoneCountries_ShippingZoneId",
                table: "ShippingZoneCountries",
                column: "ShippingZoneId");
        }
    }
}
