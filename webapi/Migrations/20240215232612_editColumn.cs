using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webapi.Migrations
{
    /// <inheritdoc />
    public partial class editColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_notifications_users_receiver_id",
                table: "notifications");

            migrationBuilder.RenameColumn(
                name: "receiver_id",
                table: "notifications",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_notifications_receiver_id",
                table: "notifications",
                newName: "IX_notifications_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_users_user_id",
                table: "notifications",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_notifications_users_user_id",
                table: "notifications");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "notifications",
                newName: "receiver_id");

            migrationBuilder.RenameIndex(
                name: "IX_notifications_user_id",
                table: "notifications",
                newName: "IX_notifications_receiver_id");

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_users_receiver_id",
                table: "notifications",
                column: "receiver_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
