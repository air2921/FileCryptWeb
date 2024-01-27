using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webapi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAPI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_used",
                table: "links");

            migrationBuilder.AddColumn<DateTime>(
                name: "expiry_date",
                table: "api",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_blocked",
                table: "api",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_time_activity",
                table: "api",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "api",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "expiry_date",
                table: "api");

            migrationBuilder.DropColumn(
                name: "is_blocked",
                table: "api");

            migrationBuilder.DropColumn(
                name: "last_time_activity",
                table: "api");

            migrationBuilder.DropColumn(
                name: "type",
                table: "api");

            migrationBuilder.AddColumn<bool>(
                name: "is_used",
                table: "links",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
