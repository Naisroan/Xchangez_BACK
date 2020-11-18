using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xchangez.Models
{
    public class Mensaje
    {
        public int Id { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public int IdGrupo { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR(250)")]
        public string Contenido { get; set; }

        [Column(TypeName = "VARCHAR(250)")]
        public string RutaContenido { get; set; }

        [Required]
        public DateTime FechaAlta { get; set; }
    }
}
