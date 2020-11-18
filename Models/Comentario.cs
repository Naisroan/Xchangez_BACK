using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xchangez.Models
{
    public class Comentario
    {
        public int Id { get; set; }

        public int IdComentarioPadre { get; set; }

        [Required]
        public int IdPublicacion { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "VARCHAR(250)")]
        public string Contenido { get; set; }

        [Required]
        public DateTime FechaAlta { get; set; }
    }
}
