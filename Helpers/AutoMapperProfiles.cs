using AutoMapper;
using System;
using Xchangez.DTOs;
using Xchangez.Models;

namespace Xchangez.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Usuario, UsuarioDTO>().ReverseMap();
        }
    }
}
