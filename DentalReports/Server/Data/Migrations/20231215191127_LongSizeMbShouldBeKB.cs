using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentalReports.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class LongSizeMbShouldBeKB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "sizeMB",
                table: "PatientFiles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "sizeMB",
                table: "PatientFiles",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
