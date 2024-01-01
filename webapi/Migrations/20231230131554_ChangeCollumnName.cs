using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webapi.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCollumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "received_internal_key",
                table: "keys",
                newName: "received_key");

            migrationBuilder.RenameColumn(
                name: "person_internal_key",
                table: "keys",
                newName: "internal_key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "received_key",
                table: "keys",
                newName: "received_internal_key");

            migrationBuilder.RenameColumn(
                name: "internal_key",
                table: "keys",
                newName: "person_internal_key");
        }
    }
}
