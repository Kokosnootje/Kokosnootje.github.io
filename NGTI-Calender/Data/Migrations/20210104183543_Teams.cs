using Microsoft.EntityFrameworkCore.Migrations;

namespace NGTI_Calender.Migrations
{
    public partial class Teams : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Person_Role_RolesId",
                table: "Person");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Person_Role_RolesId",
                table: "Person",
                column: "RolesId",
                principalTable: "Role",
                principalColumn: "RolesId",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_Reservation_Person_PersonId",
                table: "Reservation");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_Timeslot_TimeslotId",
                table: "Reservation");

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
