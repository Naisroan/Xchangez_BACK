using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xchangez.DTOs
{
    public class SeguidorDTO
    {
        public int Id { get; set; }

        public int IdUsuarioSeguidor { get; set; }

        public int IdUsuarioSeguido { get; set; }

        public string NombreCompletoSeguidor { get; set; }

        public string RutaAvatarSeguidor { get; set; }

        public string NombreCompletoSeguido { get; set; }

        public string RutaAvatarSeguido { get; set; }

        public DateTime FechaAlta { get; set; }
    }
}
