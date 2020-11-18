using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xchangez.Models
{
    public class MensajeDTO
    {
        public int Id { get; set; }

        public int IdUsuario { get; set; }

        public int IdGrupo { get; set; }

        public string Contenido { get; set; }

        public string RutaContenido { get; set; }

        public DateTime FechaAlta { get; set; }
    }
}
