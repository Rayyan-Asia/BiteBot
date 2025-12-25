using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiteBot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RestaurantAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ChangeDetails = table.Column<string>(type: "text", nullable: true),
                    ChangeDescription = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantAuditLogs_RestaurantId",
                table: "RestaurantAuditLogs",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantAuditLogs_Timestamp",
                table: "RestaurantAuditLogs",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RestaurantAuditLogs");
        }
    }
}
