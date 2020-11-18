using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xchangez.Models
{
    public class ObjetoLista
    {
        public int Id { get; set; }

        [Required]
        public int IdLista { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "VARCHAR(50)")]
        public string Nombre { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "VARCHAR(250)")]
        public string Descripcion { get; set; }
   
        [Required]
        public bool LoBusca { get; set; }
    }
}
