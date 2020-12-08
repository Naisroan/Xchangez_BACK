using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xchangez.DTOs;
using Xchangez.Interfaces;
using Xchangez.Models;

namespace Xchangez.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // esto indica que necesita autorización por token
    public class SeguidorController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        private readonly IRepository<Seguidor, SeguidorDTO> Repository;
        private readonly IRepository<Usuario, UsuarioDTO> RepositoryUsuario;

        public SeguidorController(IConfiguration configuration, IRepository<Seguidor, SeguidorDTO> repository,
            IRepository<Usuario, UsuarioDTO> repositoryUsuario)
        {
            Configuration = configuration;

            Repository = repository;
            RepositoryUsuario = repositoryUsuario;
        }

        /// <summary>
        /// Se crea un registro de seguimiento del usuario autenticado al que se pase por parámetro
        /// </summary>
        /// <param name="id">Id del usuario a seguir</param>
        /// <returns>El objet de Seguimiento creado junto a su ID</returns>
        [HttpPost("Create")]
        public async Task<ActionResult> Create(int id)
        {
            try
            {
                // obtenemos el usuario autenticado (mediante token)
                Usuario authUser = await Fun.GetAuthUser(Repository.GetContext(), User);

                if (authUser == null)
                {
                    return NotFound("El usuario autenticado no existe o no se logró obtener su información");
                }

                // obtenemos el usuario a seguir
                UsuarioDTO usuarioSeguido = (await RepositoryUsuario.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (usuarioSeguido == null)
                {
                    return NotFound("El usuario a seguir no existe o no se logró obtener su información");
                }

                // validamos que no se pueda seguir a si mismo
                if (usuarioSeguido.Id == authUser.Id)
                {
                    return BadRequest("El usuario a seguir esta intentando seguirse a si mismo");
                }

                // obtenemos el objeto de segumiento filtrando por el usuario seguidor y el usuario seguido
                var seguimientos = await Repository.GetAsync(n => n.IdUsuarioSeguido == usuarioSeguido.Id && n.IdUsuarioSeguidor == authUser.Id);
                SeguidorDTO dto = seguimientos.FirstOrDefault();

                if (dto != null)
                {
                    return BadRequest("El usuario autenticado ya sigue a al usuario a seguir");
                }

                // creamos un objeto
                Seguidor nuevo = new Seguidor()
                {
                    IdUsuarioSeguidor = authUser.Id,
                    IdUsuarioSeguido = usuarioSeguido.Id,
                    FechaAlta = DateTime.Now
                };

                SeguidorDTO seguidorDTO = Repository.GetMapper().Map<SeguidorDTO>(nuevo);

                // creamos y guardamos
                await Repository.CreateAsync(nuevo);
                await Repository.Commit();

                seguidorDTO.NombreCompletoSeguidor = $"{authUser.Nombre} {authUser.Apellido}";
                seguidorDTO.RutaAvatarSeguidor = authUser.RutaImagenAvatar;

                seguidorDTO.NombreCompletoSeguido = $"{usuarioSeguido.Nombre} {usuarioSeguido.Apellido}";
                seguidorDTO.RutaAvatarSeguido = usuarioSeguido.RutaImagenAvatar;

                // retornamos la publicacion con su nuevo id
                return new CreatedAtRouteResult("GetSeguidor", new
                {
                    id = nuevo.Id
                }, seguidorDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene un objeto con información del seguimiento de un usuario a otro
        /// </summary>
        /// <param name="id">Id del seguimiento</param>
        /// <returns>Seguidor</returns>
        [HttpGet("{id:int}", Name = "GetSeguidor")]
        [AllowAnonymous]
        public async Task<ActionResult<SeguidorDTO>> Get(int id)
        {
            try
            {
                SeguidorDTO nodo = (await Repository.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (nodo == null)
                {
                    return NotFound("Seguidor no encontrado"); // retorna un codigo de error 404
                }

                await Repository.Commit();

                return nodo;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un seguimiento del usuario autenticado a otro especificado por parámetro
        /// </summary>
        /// <param name="id">Id del usuario seguido</param>
        /// <returns>Estado de success 204</returns>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                // obtenemos el usuario autenticado (mediante token)
                Usuario authUser = await Fun.GetAuthUser(Repository.GetContext(), User);

                if (authUser == null)
                {
                    return NotFound("El usuario autenticado no existe o no se logró obtener su información");
                }

                // obtenemos el usuario a seguir
                UsuarioDTO usuarioSeguido = (await RepositoryUsuario.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (usuarioSeguido == null)
                {
                    return NotFound("El usuario a seguir no existe o no se logró obtener su información");
                }

                // obtenemos el objeto de segumiento filtrando por el usuario seguidor y el usuario seguido
                SeguidorDTO dto = (await Repository.GetAsync(n => n.IdUsuarioSeguido == usuarioSeguido.Id && n.IdUsuarioSeguidor == authUser.Id)).FirstOrDefault();

                if (dto == null)
                {
                    return NotFound("No se encontró la información de seguimiento a eliminar"); // retorna un codigo de error 404
                }

                Seguidor nodo = Repository.GetMapper().Map<Seguidor>(dto);

                Repository.Delete(nodo);
                await Repository.Commit();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Se obtienen todos los seguidores de un usuario (Si el usuario es privado, no regresa nada)
        /// </summary>
        /// <returns>Lista de publicaciones</returns>
        [HttpGet("SeguidoresByIdUsuario")]
        [AllowAnonymous]
        public async Task<ActionResult<List<SeguidorDTO>>> GetSeguidoresByIdUsuario(int id)
        {
            try
            {
                Usuario authUser = await Fun.GetAuthUser(Repository.GetContext(), User);
                UsuarioDTO usuario = (await RepositoryUsuario.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (usuario == null)
                {
                    return NotFound("El usuario no existe o no se logró obtener su información");
                }

                // validamos si los seguidores los esta pidiendo para el mismo usuario autenticado
                bool sonDeUsuarioAutenticado = authUser != null && authUser.Id == usuario.Id;

                if (!sonDeUsuarioAutenticado && usuario.EsPrivado.GetValueOrDefault(false))
                {
                    return new List<SeguidorDTO>();
                }

                var seguimientos = await Repository.GetAsync(n => n.IdUsuarioSeguido == id);
                await LlenarInformacionExtra(seguimientos);

                return seguimientos;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Se obtienen todos los seguimientos de un usuario (Si el usuario es privado, no regresa nada)
        /// </summary>
        /// <returns>Lista de publicaciones</returns>
        [HttpGet("SeguimientosByIdUsuario")]
        [AllowAnonymous]
        public async Task<ActionResult<List<SeguidorDTO>>> GetSeguimientosByIdUsuario(int id)
        {
            try
            {
                Usuario authUser = await Fun.GetAuthUser(Repository.GetContext(), User);
                UsuarioDTO usuario = (await RepositoryUsuario.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (usuario == null)
                {
                    return NotFound("El usuario no existe o no se logró obtener su información");
                }

                // validamos si los seguidores los esta pidiendo para el mismo usuario autenticado
                bool sonDeUsuarioAutenticado = authUser != null && authUser.Id == usuario.Id;

                if (!sonDeUsuarioAutenticado && usuario.EsPrivado.GetValueOrDefault(false))
                {
                    return new List<SeguidorDTO>();
                }

                var seguimientos = await Repository.GetAsync(n => n.IdUsuarioSeguidor == id);
                await LlenarInformacionExtra(seguimientos);

                return seguimientos;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Verifica si el usuario autenticado esta siguiendo al usuario especificado
        /// </summary>
        /// <returns>True o False</returns>
        [HttpGet("EstaSiguiendoloByIdUsuario")]
        public async Task<ActionResult<bool>> GetEstaSiguiendoloByIdUsuario(int id)
        {
            try
            {
                Usuario authUser = await Fun.GetAuthUser(Repository.GetContext(), User);
                UsuarioDTO usuarioSeguido = (await RepositoryUsuario.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (authUser == null)
                {
                    return NotFound("El usuario no esta autenticado");
                }

                if (usuarioSeguido == null)
                {
                    return NotFound("El usuario a verificar a seguir no existe o no se logró obtener su información");
                }

                var seguimientos = await Repository.GetAsync(n => n.IdUsuarioSeguidor == authUser.Id && n.IdUsuarioSeguido == id);
                return seguimientos.Count > 0;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task LlenarInformacionExtra(List<SeguidorDTO> seguimientos)
        {
            if (seguimientos == null || seguimientos.Count <= 0)
            {
                return;
            }

            foreach (SeguidorDTO seguimiento in seguimientos)
            {
                UsuarioDTO usuarioSeguidor = (await RepositoryUsuario.GetAsync(n => n.Id == seguimiento.IdUsuarioSeguidor)).FirstOrDefault();
                UsuarioDTO usuarioSeguido = (await RepositoryUsuario.GetAsync(n => n.Id == seguimiento.IdUsuarioSeguido)).FirstOrDefault();

                if (usuarioSeguidor != null)
                {
                    seguimiento.NombreCompletoSeguidor = $"{usuarioSeguidor.Nombre} {usuarioSeguidor.Apellido}";
                    seguimiento.RutaAvatarSeguidor = usuarioSeguidor.RutaImagenAvatar;
                }

                if (usuarioSeguido != null)
                {
                    seguimiento.NombreCompletoSeguido = $"{usuarioSeguido.Nombre} {usuarioSeguido.Apellido}";
                    seguimiento.RutaAvatarSeguido = usuarioSeguido.RutaImagenAvatar;
                }
            }
        }
    }
}
