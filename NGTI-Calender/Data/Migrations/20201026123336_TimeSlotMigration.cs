using Microsoft.EntityFrameworkCore.Migrations;

namespace NGTI_Calender.Data.Migrations
{
    public partial class TimeSlotMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimeslotId",
                table: "Reservation",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Timeslot",
                columns: table => new
                {
                    TimeslotId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeStart = table.Column<string>(nullable: true),
                    TimeEnd = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timeslot", x => x.TimeslotId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_TimeslotId",
                table: "Reservation",
                column: "TimeslotId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_Timeslot_TimeslotId",
                table: "Reservation",
                column: "TimeslotId",
                principalTable: "Timeslot",
                principalColumn: "TimeslotId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_Timeslot_TimeslotId",
                table: "Reservation");

            migrationBuilder.DropTable(
                name: "Timeslot");

            migrationBuilder.DropIndex(
                name: "IX_Reservation_TimeslotId",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "TimeslotId",
                table: "Reservation");
        }
    }
}
