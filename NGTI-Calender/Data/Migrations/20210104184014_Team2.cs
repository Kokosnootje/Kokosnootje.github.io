using Microsoft.EntityFrameworkCore.Migrations;

namespace NGTI_Calender.Migrations
{
    public partial class Team2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "Person",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    TeamId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.TeamId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Person_TeamId",
                table: "Person",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Person_Teams_TeamId",
                table: "Person",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Person_Teams_TeamId",
                table: "Person");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Person_TeamId",
                table: "Person");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Person");
        }
    }
}
