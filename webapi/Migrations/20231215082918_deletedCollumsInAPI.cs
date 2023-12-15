using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webapi.Migrations
{
    /// <inheritdoc />
    public partial class deletedCollumsInAPI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_allowed_unknown_ip",
                table: "api");

            migrationBuilder.DropColumn(
                name: "is_tracking_ip",
                table: "api");

            migrationBuilder.DropColumn(
                name: "remote_ip",
                table: "api");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_allowed_unknown_ip",
                table: "api",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_tracking_ip",
                table: "api",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<IPAddress>(
                name: "remote_ip",
                table: "api",
                type: "inet",
                nullable: true);
        }
    }
}
