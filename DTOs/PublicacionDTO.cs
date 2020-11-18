using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xchangez.Models
{
    public class PublicacionDTO
    {
        public int Id { get; set; }

        public int IdUsuario { get; set; }

        public string Titulo { get; set; }

        public string Descripcion { get; set; }

        public string Caracteristicas { get; set; }

        public bool EsBorrador { get; set; }

        public float Precio { get; set; }

        public int Estado { get; set; }

        public DateTime FechaAlta { get; set; }

        public DateTime? FechaModificacion { get; set; }
    }
}
