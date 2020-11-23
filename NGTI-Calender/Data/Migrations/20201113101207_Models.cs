using Microsoft.EntityFrameworkCore.Migrations;

namespace NGTI_Calender.Data.Migrations
{
    public partial class Models : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Person");

            migrationBuilder.AddColumn<string>(
                name: "EMail",
                table: "Person",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EMail",
                table: "Person");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Person",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
