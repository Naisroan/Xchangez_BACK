using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xchangez.Models
{
    public class Lista
    {
        public int Id { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "VARCHAR(100)")]
        public string Nombre { get; set; }

        [Column(TypeName = "VARCHAR(250)")]
        public string Descripcion { get; set; }

        [Required]
        public bool EsPublico { get; set; }
    }
}
