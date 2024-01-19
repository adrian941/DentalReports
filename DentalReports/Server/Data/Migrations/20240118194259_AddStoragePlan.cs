using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentalReports.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStoragePlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StoragePlan",
                table: "Technicians",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StoragePlan",
                table: "Doctors",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoragePlan",
                table: "Technicians");

            migrationBuilder.DropColumn(
                name: "StoragePlan",
                table: "Doctors");
        }
    }
}
