using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xchangez.Models
{
    public class ListaDTO
    {
        public int Id { get; set; }

        public int IdUsuario { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public bool EsPublico { get; set; }

        public List<ObjetoListaDTO> Objetos { get; set; }
    }
}
