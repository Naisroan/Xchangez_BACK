using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xchangez.Models
{
    public class ComentarioDTO
    {
        public int Id { get; set; }

        public int IdComentarioPadre { get; set; }

        public int IdPublicacion { get; set; }

        public int IdUsuario { get; set; }

        public string NombreCompleto { get; set; }

        public string RutaImagenAvatar { get; set; }

        public string Contenido { get; set; }

        public DateTime FechaAlta { get; set; }

        public List<ComentarioDTO> Comentarios { get; set; }
    }
}
