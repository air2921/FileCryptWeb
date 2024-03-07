using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace webapi.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "allowedmime",
                columns: table => new
                {
                    mime_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mime_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_allowedmime", x => x.mime_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    is_2fa_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    is_blocked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "api",
                columns: table => new
                {
                    api_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    api_key = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_blocked = table.Column<bool>(type: "boolean", nullable: false),
                    last_time_activity = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    max_request_of_day = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api", x => x.api_id);
                    table.ForeignKey(
                        name: "FK_api_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "files",
                columns: table => new
                {
                    file_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    file_mime = table.Column<string>(type: "text", nullable: false),
                    file_mime_category = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    operation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_files", x => x.file_id);
                    table.ForeignKey(
                        name: "FK_files_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "keys",
                columns: table => new
                {
                    key_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    private_key = table.Column<string>(type: "text", nullable: false),
                    internal_key = table.Column<string>(type: "text", nullable: true),
                    received_key = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_keys", x => x.key_id);
                    table.ForeignKey(
                        name: "FK_keys_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "links",
                columns: table => new
                {
                    link_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    u_token = table.Column<string>(type: "text", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_links", x => x.link_id);
                    table.ForeignKey(
                        name: "FK_links_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    notification_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    message_header = table.Column<string>(type: "text", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    priority = table.Column<string>(type: "text", nullable: false),
                    send_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_checked = table.Column<bool>(type: "boolean", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.notification_id);
                    table.ForeignKey(
                        name: "FK_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "offers",
                columns: table => new
                {
                    offer_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    offer_header = table.Column<string>(type: "text", nullable: false),
                    offer_body = table.Column<string>(type: "text", nullable: false),
                    offer_type = table.Column<string>(type: "text", nullable: false),
                    is_accepted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sender_id = table.Column<int>(type: "integer", nullable: false),
                    receiver_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_offers", x => x.offer_id);
                    table.ForeignKey(
                        name: "FK_offers_users_receiver_id",
                        column: x => x.receiver_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_offers_users_sender_id",
                        column: x => x.sender_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "storages",
                columns: table => new
                {
                    storage_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    storage_name = table.Column<string>(type: "text", nullable: false),
                    last_time_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    access_code = table.Column<string>(type: "text", nullable: false),
                    encrypt = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "tokens",
                columns: table => new
                {
                    token_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    refresh_token = table.Column<string>(type: "text", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tokens", x => x.token_id);
                    table.ForeignKey(
                        name: "FK_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "storage_items",
                columns: table => new
                {
                    key_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                name: "IX_api_api_key",
                table: "api",
                column: "api_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_api_user_id",
                table: "api",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_files_user_id",
                table: "files",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_keys_user_id",
                table: "keys",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_links_u_token",
                table: "links",
                column: "u_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_links_user_id",
                table: "links",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_offers_receiver_id",
                table: "offers",
                column: "receiver_id");

            migrationBuilder.CreateIndex(
                name: "IX_offers_sender_id",
                table: "offers",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_storage_items_storage_id",
                table: "storage_items",
                column: "storage_id");

            migrationBuilder.CreateIndex(
                name: "IX_storages_user_id",
                table: "storages",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tokens_refresh_token",
                table: "tokens",
                column: "refresh_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tokens_user_id",
                table: "tokens",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "allowedmime");

            migrationBuilder.DropTable(
                name: "api");

            migrationBuilder.DropTable(
                name: "files");

            migrationBuilder.DropTable(
                name: "keys");

            migrationBuilder.DropTable(
                name: "links");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "offers");

            migrationBuilder.DropTable(
                name: "storage_items");

            migrationBuilder.DropTable(
                name: "tokens");

            migrationBuilder.DropTable(
                name: "storages");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
