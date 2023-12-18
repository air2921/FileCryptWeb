using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webapi.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexTokensLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_tokens_refresh_token",
                table: "tokens",
                column: "refresh_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_links_u_token",
                table: "links",
                column: "u_token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tokens_refresh_token",
                table: "tokens");

            migrationBuilder.DropIndex(
                name: "IX_links_u_token",
                table: "links");
        }
    }
}
