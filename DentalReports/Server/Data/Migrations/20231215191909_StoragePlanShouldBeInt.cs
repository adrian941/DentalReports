using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentalReports.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class StoragePlanShouldBeInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "sizeStoragePlanGB",
                table: "Technicians",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "sizeStoragePlanGB",
                table: "Technicians",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
