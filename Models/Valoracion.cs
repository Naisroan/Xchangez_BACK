using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xchangez.Models
{
    public class Valoracion
    {
        public int Id { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public int IdUsuarioValorado { get; set; }

        [Required]
        public int Cantidad { get; set; }

        public DateTime FechaAlta { get; set; }
    }
}
