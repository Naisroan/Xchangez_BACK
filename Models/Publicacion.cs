using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xchangez.Models
{
    public class Publicacion
    {
        public int Id { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "VARCHAR(50)")]
        public string Titulo { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "VARCHAR(250)")]
        public string Descripcion { get; set; }

        [Column(TypeName = "VARCHAR(MAX)")]
        public string Caracteristicas { get; set; }

        [Required]
        public bool EsBorrador { get; set; }

        [Required]
        public float Precio { get; set; }

        [Required]
        public int Estado { get; set; }

        [Required]
        public DateTime FechaAlta { get; set; }

        public DateTime? FechaModificacion { get; set; }
    }
}
