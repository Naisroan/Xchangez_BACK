using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xchangez.Models
{
    public class Multimedia
    {
        public int Id { get; set; }

        [Required]
        public int IdPublicacion { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "VARCHAR(250)")]
        public string Ruta { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "VARCHAR(50)")]
        public string Nombre { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "VARCHAR(10)")]
        public string Extension { get; set; }

        [Required]
        public DateTime FechaAlta { get; set; }
    }
}
