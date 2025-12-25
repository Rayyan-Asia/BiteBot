using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiteBot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCityNameUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_Name_City",
                table: "Restaurants",
                columns: new[] { "Name", "City" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Restaurants_Name_City",
                table: "Restaurants");
        }
    }
}
