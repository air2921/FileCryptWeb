using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webapi.Migrations
{
    /// <inheritdoc />
    public partial class ApiRelationsUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_api_user_id",
                table: "api");

            migrationBuilder.CreateIndex(
                name: "IX_api_user_id",
                table: "api",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_api_user_id",
                table: "api");

            migrationBuilder.CreateIndex(
                name: "IX_api_user_id",
                table: "api",
                column: "user_id",
                unique: true);
        }
    }
}
