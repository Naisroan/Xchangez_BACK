using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xchangez.Models
{
    public class ObjetoListaDTO
    {
        public int Id { get; set; }

        public int IdLista { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }
   
        public bool LoBusca { get; set; }
    }
}
