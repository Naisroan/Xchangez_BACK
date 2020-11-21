using Microsoft.EntityFrameworkCore.Migrations;

namespace Xchangez.Migrations
{
    public partial class Xchangez_bd_102 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Publicaciones",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EsTrueque",
                table: "Publicaciones",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Visitas",
                table: "Publicaciones",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Publicaciones");

            migrationBuilder.DropColumn(
                name: "EsTrueque",
                table: "Publicaciones");

            migrationBuilder.DropColumn(
                name: "Visitas",
                table: "Publicaciones");
        }
    }
}
