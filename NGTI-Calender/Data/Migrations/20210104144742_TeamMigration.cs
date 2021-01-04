using Microsoft.EntityFrameworkCore.Migrations;

namespace NGTI_Calender.Migrations
{
    public partial class TeamMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_Person_PersonId",
                table: "Reservation");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_Timeslot_TimeslotId",
                table: "Reservation");

            migrationBuilder.AlterColumn<int>(
                name: "TimeslotId",
                table: "Reservation",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PersonId",
                table: "Reservation",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RolesId",
                table: "Person",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "Person",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Team",
                columns: table => new
                {
                    TeamId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Team", x => x.TeamId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Person_TeamId",
                table: "Person",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Person_Role_RolesId",
                table: "Person",
                column: "RolesId",
                principalTable: "Role",
                principalColumn: "RolesId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Person_Team_TeamId",
                table: "Person",
                column: "TeamId",
                principalTable: "Team",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_Person_PersonId",
                table: "Reservation",
                column: "PersonId",
                principalTable: "Person",
                principalColumn: "PersonId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_Timeslot_TimeslotId",
                table: "Reservation",
                column: "TimeslotId",
                principalTable: "Timeslot",
                principalColumn: "TimeslotId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Person_Role_RolesId",
                table: "Person");

            migrationBuilder.DropForeignKey(
                name: "FK_Person_Team_TeamId",
                table: "Person");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_Person_PersonId",
                table: "Reservation");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_Timeslot_TimeslotId",
                table: "Reservation");

            migrationBuilder.DropTable(
                name: "Team");

            migrationBuilder.DropIndex(
                name: "IX_Person_TeamId",
                table: "Person");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Person");

            migrationBuilder.AlterColumn<int>(
                name: "TimeslotId",
                table: "Reservation",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "PersonId",
                table: "Reservation",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "RolesId",
                table: "Person",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_Person_Role_RolesId",
                table: "Person",
                column: "RolesId",
                principalTable: "Role",
                principalColumn: "RolesId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_Person_PersonId",
                table: "Reservation",
                column: "PersonId",
                principalTable: "Person",
                principalColumn: "PersonId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_Timeslot_TimeslotId",
                table: "Reservation",
                column: "TimeslotId",
                principalTable: "Timeslot",
                principalColumn: "TimeslotId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
