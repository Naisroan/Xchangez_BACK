using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xchangez.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "VARCHAR(25)")]
        public string Nick { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "VARCHAR(50)")]
        public string Nombre { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "VARCHAR(50)")]
        public string Apellido { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "VARCHAR(25)")]
        public string Password { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "VARCHAR(50)")]
        public string Correo { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        [Column(TypeName = "VARCHAR(250)")]
        public string RutaImagenAvatar { get; set; }

        [Column(TypeName = "VARCHAR(250)")]
        public string RutaImagenPortada { get; set; }

        public float? Valoracion { get; set; }

        public bool? EsPrivado { get; set; }
    }
}
