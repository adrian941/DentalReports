using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentalReports.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class LongSizeMbShouldBeBytes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "sizeMB",
                table: "PatientFiles",
                newName: "sizeBytes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "sizeBytes",
                table: "PatientFiles",
                newName: "sizeMB");
        }
    }
}
