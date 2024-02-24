using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webapi.Migrations
{
    /// <inheritdoc />
    public partial class storage_relations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_storages_user_id",
                table: "storages");

            migrationBuilder.AddColumn<bool>(
                name: "encrypt",
                table: "storages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "storage_name",
                table: "storages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_storages_user_id",
                table: "storages",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_storages_user_id",
                table: "storages");

            migrationBuilder.DropColumn(
                name: "encrypt",
                table: "storages");

            migrationBuilder.DropColumn(
                name: "storage_name",
                table: "storages");

            migrationBuilder.CreateIndex(
                name: "IX_storages_user_id",
                table: "storages",
                column: "user_id",
                unique: true);
        }
    }
}
