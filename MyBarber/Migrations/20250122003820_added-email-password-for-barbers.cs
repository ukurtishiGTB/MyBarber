using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyBarber.Migrations
{
    /// <inheritdoc />
    public partial class addedemailpasswordforbarbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Barbers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HashPassword",
                table: "Barbers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Barbers");

            migrationBuilder.DropColumn(
                name: "HashPassword",
                table: "Barbers");
        }
    }
}
