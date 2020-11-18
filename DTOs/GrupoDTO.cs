using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xchangez.Models
{
    public class GrupoDTO
    {
        public int Id { get; set; }

        public string Nombre { get; set; }

        public int IdUsuario { get; set; }

        public DateTime FechaAlta { get; set; }
    }
}
