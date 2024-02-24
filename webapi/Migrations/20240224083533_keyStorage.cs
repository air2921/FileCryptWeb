using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace webapi.Migrations
{
    /// <inheritdoc />
    public partial class keyStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "storages",
                columns: table => new
                {
                    storage_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    last_time_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    access_code = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_storages", x => x.storage_id);
                    table.ForeignKey(
                        name: "FK_storages_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "storage_items",
                columns: table => new
                {
                    key_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    key_name = table.Column<string>(type: "text", nullable: false),
                    key_value = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    storage_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_storage_items", x => x.key_id);
                    table.ForeignKey(
                        name: "FK_storage_items_storages_storage_id",
                        column: x => x.storage_id,
                        principalTable: "storages",
                        principalColumn: "storage_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_storage_items_storage_id",
                table: "storage_items",
                column: "storage_id");

            migrationBuilder.CreateIndex(
                name: "IX_storages_user_id",
                table: "storages",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "storage_items");

            migrationBuilder.DropTable(
                name: "storages");
        }
    }
}
