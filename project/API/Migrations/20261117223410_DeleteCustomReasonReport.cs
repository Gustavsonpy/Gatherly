using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class DeleteCustomReasonReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomReason",
                table: "UserReports");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserRate_RateRange",
                table: "UserRating",
                sql: "\"Rate\" BETWEEN 1 AND 5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_UserRate_RateRange",
                table: "UserRating");

            migrationBuilder.AddColumn<string>(
                name: "CustomReason",
                table: "UserReports",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
