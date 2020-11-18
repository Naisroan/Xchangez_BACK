using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Xchangez.Migrations
{
    public partial class Xchangez_bd_101 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Comentarios",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdComentarioPadre = table.Column<int>(nullable: false),
                    IdPublicacion = table.Column<int>(nullable: false),
                    IdUsuario = table.Column<int>(nullable: false),
                    Contenido = table.Column<string>(type: "VARCHAR(250)", nullable: false),
                    FechaAlta = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comentarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Grupos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    IdUsuario = table.Column<int>(nullable: false),
                    FechaAlta = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grupos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Listas",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(nullable: false),
                    Nombre = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    EsPublico = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Listas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mensajes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(nullable: false),
                    IdGrupo = table.Column<int>(nullable: false),
                    Contenido = table.Column<string>(type: "VARCHAR(250)", nullable: false),
                    RutaContenido = table.Column<string>(type: "VARCHAR(250)", nullable: true),
                    FechaAlta = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mensajes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Multimedias",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPublicacion = table.Column<int>(nullable: false),
                    Ruta = table.Column<string>(type: "VARCHAR(250)", nullable: false),
                    Nombre = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    Extension = table.Column<string>(type: "VARCHAR(10)", nullable: false),
                    FechaAlta = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Multimedias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ObjetosListas",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdLista = table.Column<int>(nullable: false),
                    Nombre = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    Descripcion = table.Column<string>(type: "VARCHAR(250)", nullable: false),
                    LoBusca = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjetosListas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Publicaciones",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(nullable: false),
                    Titulo = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    Descripcion = table.Column<string>(type: "VARCHAR(250)", nullable: false),
                    Caracteristicas = table.Column<string>(type: "VARCHAR(MAX)", nullable: true),
                    EsBorrador = table.Column<bool>(nullable: false),
                    Precio = table.Column<float>(nullable: false),
                    Estado = table.Column<int>(nullable: false),
                    FechaAlta = table.Column<DateTime>(nullable: false),
                    FechaModificacion = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publicaciones", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comentarios");

            migrationBuilder.DropTable(
                name: "Grupos");

            migrationBuilder.DropTable(
                name: "Listas");

            migrationBuilder.DropTable(
                name: "Mensajes");

            migrationBuilder.DropTable(
                name: "Multimedias");

            migrationBuilder.DropTable(
                name: "ObjetosListas");

            migrationBuilder.DropTable(
                name: "Publicaciones");
        }
    }
}
