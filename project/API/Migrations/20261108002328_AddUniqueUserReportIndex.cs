using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueUserReportIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserReports_ReporterId",
                table: "UserReports");

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_Reporter_Reported_Event_Reason",
                table: "UserReports",
                columns: new[] { "ReporterId", "ReportedUserId", "EventId", "Reason" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserReports_Reporter_Reported_Event_Reason",
                table: "UserReports");

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_ReporterId",
                table: "UserReports",
                column: "ReporterId");
        }
    }
}
