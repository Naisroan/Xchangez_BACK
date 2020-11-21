using AutoMapper;
using System;
using Xchangez.DTOs;
using Xchangez.Models;

namespace Xchangez.Helpers
{
    /// <summary>
    /// Aquí se asignan los mapeos de las clases abstractas a sus respectivos DTOs
    /// </summary>
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Publicacion, PublicacionDTO>().ReverseMap();
            CreateMap<Usuario, UsuarioDTO>().ReverseMap();
            CreateMap<ObjetoLista, ObjetoListaDTO>().ReverseMap();
            CreateMap<Multimedia, MultimediaDTO>().ReverseMap();
            CreateMap<Mensaje, MensajeDTO>().ReverseMap();
            CreateMap<Lista, ListaDTO>().ReverseMap();
            CreateMap<Grupo, GrupoDTO>().ReverseMap();
            CreateMap<Comentario, ComentarioDTO>().ReverseMap();
            CreateMap<Valoracion, ValoracionDTO>().ReverseMap();
        }
    }
}
