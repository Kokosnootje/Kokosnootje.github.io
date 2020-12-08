using Microsoft.EntityFrameworkCore.Migrations;

namespace NGTI_Calender.Data.Migrations
{
    public partial class ImageInPerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Person",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Person");
        }
    }
}
