using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripNow_JoseAngel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCountryToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TripCountry",
                table: "Reservations",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TripCountry",
                table: "Reservations");
        }
    }
}
