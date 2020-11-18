using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xchangez.Models
{
    public class Grupo
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "VARCHAR(100)")]
        public string Nombre { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public DateTime FechaAlta { get; set; }
    }
}
