using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xchangez.DTOs
{
    public class UsuarioDTO
    {
        public int Id { get; set; }

        public string Nick { get; set; }

        public string Nombre { get; set; }

        public string Apellido { get; set; }

        public string Password { get; set; }

        public string Correo { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        public string RutaImagenAvatar { get; set; }

        public string RutaImagenPortada { get; set; }

        public float? Valoracion { get; set; }

        public bool? EsPrivado { get; set; }

        public int CantidadSeguidores { get; set; }

        public int CantidadSeguidos { get; set; }
    }
}
