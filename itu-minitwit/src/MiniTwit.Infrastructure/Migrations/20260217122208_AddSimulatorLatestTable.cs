using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chirp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSimulatorLatestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthorAchievements_Achievement_AchievementId",
                table: "AuthorAchievements");

            migrationBuilder.DropTable(
                name: "Achievement");

            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    AchievementId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 25, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ImagePath = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.AchievementId);
                });

            migrationBuilder.CreateTable(
                name: "Latest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Value = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Latest", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Name",
                table: "AspNetUsers",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuthorAchievements_Achievements_AchievementId",
                table: "AuthorAchievements",
                column: "AchievementId",
                principalTable: "Achievements",
                principalColumn: "AchievementId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthorAchievements_Achievements_AchievementId",
                table: "AuthorAchievements");

            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropTable(
                name: "Latest");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Name",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "Achievement",
                columns: table => new
                {
                    AchievementId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ImagePath = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievement", x => x.AchievementId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AuthorAchievements_Achievement_AchievementId",
                table: "AuthorAchievements",
                column: "AchievementId",
                principalTable: "Achievement",
                principalColumn: "AchievementId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
