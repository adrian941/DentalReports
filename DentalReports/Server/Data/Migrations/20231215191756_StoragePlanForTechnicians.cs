using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentalReports.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class StoragePlanForTechnicians : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "sizeStoragePlanGB",
                table: "Technicians",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sizeStoragePlanGB",
                table: "Technicians");
        }
    }
}
