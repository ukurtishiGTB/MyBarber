using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyBarber.Migrations
{
    /// <inheritdoc />
    public partial class seedbarbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Barbers",
                columns: new[] { "Id", "Email", "HashPassword", "Location", "Name", "PhoneNumber", "Pricing", "ProfileImage", "Rating", "Services", "isActive" },
                values: new object[,]
                {
                    { 1, "test@gmail.com", "test", "Cair", "John Doe", "123-456-7890", 20m, null, 0.0, "Haircut", true },
                    { 2, "test@gmail.com", "test", "Cair", "Jane Smith", "123-456-7890", 20m, null, 0.0, "Haircut", true },
                    { 3, "test@gmail.com", "test", "Cair", "Mike Johnson", "123-456-7890", 20m, null, 0.0, "Haircut", true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Barbers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Barbers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Barbers",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
