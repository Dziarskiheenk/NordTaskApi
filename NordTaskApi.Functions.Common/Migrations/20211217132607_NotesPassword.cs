using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NordTaskApi.Common.Migrations
{
    public partial class NotesPassword : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Notes",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Notes");
        }
    }
}
