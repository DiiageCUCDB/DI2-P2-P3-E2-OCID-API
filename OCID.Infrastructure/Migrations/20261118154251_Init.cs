using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCID.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    client_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    client_secret = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    scopes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    roles = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    @event = table.Column<string>(name: "event", type: "nvarchar(200)", maxLength: 200, nullable: false),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_Logs_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    app_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    issued_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_Tokens_Applications_app_id",
                        column: x => x.app_id,
                        principalTable: "Applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tokens_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_client_id",
                table: "Applications",
                column: "client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Logs_timestamp",
                table: "Logs",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_user_id",
                table: "Logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_app_id",
                table: "Tokens",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_expires_at",
                table: "Tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_user_id",
                table: "Tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_email",
                table: "Users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
