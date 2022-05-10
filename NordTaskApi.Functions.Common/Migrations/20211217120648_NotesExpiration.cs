using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NordTaskApi.Common.Migrations
{
    public partial class NotesExpiration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "Notes",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Notes");
        }
    }
}
