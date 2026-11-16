using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AddEventPlaceDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "Events",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Events",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Events",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceName",
                table: "Events",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Events",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Events",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "District",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "PlaceName",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "Events");
        }
    }
}
