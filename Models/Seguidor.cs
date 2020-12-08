using System;
using System.ComponentModel.DataAnnotations;

namespace Xchangez.Models
{
    public class Seguidor
    {
        public int Id { get; set; }

        [Required]
        public int IdUsuarioSeguidor { get; set; }

        [Required]
        public int IdUsuarioSeguido { get; set; }

        [Required]
        public DateTime FechaAlta { get; set; }
    }
}
