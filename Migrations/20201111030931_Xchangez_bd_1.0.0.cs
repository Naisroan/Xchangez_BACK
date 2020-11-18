using Microsoft.EntityFrameworkCore.Migrations;

namespace Xchangez.Migrations
{
    public partial class Xchangez_bd_100 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsPrivado",
                table: "Usuarios",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nick",
                table: "Usuarios",
                type: "VARCHAR(25)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RutaImagenAvatar",
                table: "Usuarios",
                type: "VARCHAR(250)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutaImagenPortada",
                table: "Usuarios",
                type: "VARCHAR(250)",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Valoracion",
                table: "Usuarios",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EsPrivado",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Nick",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "RutaImagenAvatar",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "RutaImagenPortada",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Valoracion",
                table: "Usuarios");
        }
    }
}
