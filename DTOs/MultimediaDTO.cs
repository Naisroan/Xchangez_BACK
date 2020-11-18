using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xchangez.Models
{
    public class MultimediaDTO
    {
        public int Id { get; set; }

        public int IdPublicacion { get; set; }

        public string Ruta { get; set; }

        public string Nombre { get; set; }

        public string Extension { get; set; }

        public DateTime FechaAlta { get; set; }

        // este se agego para agregar contenido (se tiene que ignorar al mapear Helpers/AutoMapperProfiles.cs)
        // public IFormFile Contenido;
    }
}
