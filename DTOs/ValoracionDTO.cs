﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xchangez.DTOs
{
    public class ValoracionDTO
    {
        public int Id { get; set; }

        public int IdUsuario { get; set; }

        public int IdUsuarioValorado { get; set; }

        public string NombreCompleto { get; set; }

        public string RutaImagenAvatar { get; set; }

        public int Cantidad { get; set; }

        public string Comentario { get; set; }

        public DateTime FechaAlta { get; set; }
    }
}
