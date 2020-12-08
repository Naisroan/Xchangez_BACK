using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xchangez.DTOs;
using Xchangez.Interfaces;
using Xchangez.Models;

namespace Xchangez.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // esto indica que necesita autorización por token
    public class ComentarioController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        private readonly IRepository<Comentario, ComentarioDTO> Repository;
        private readonly IRepository<Publicacion, PublicacionDTO> RepositoryPublicacion;
        private readonly IRepository<Usuario, UsuarioDTO> RepositoryUsuario;

        public ComentarioController(IConfiguration configuration, IRepository<Comentario, ComentarioDTO> repository,
            IRepository<Publicacion, PublicacionDTO> repositoryPublicacion,
            IRepository<Usuario, UsuarioDTO> repositoryUsuario)
        {
            Configuration = configuration;
            Repository = repository;
            RepositoryPublicacion = repositoryPublicacion;
            RepositoryUsuario = repositoryUsuario;
        }

        /// <summary>
        /// Crea un comentario a alguna publicación en base al usuario autenticado por token
        /// </summary>
        /// <param name="idPublicacion">Id de la publicación que pertenecerá el comentario</param>
        /// <param name="comentario">Comentario creado</param>
        /// <returns>El comentario creado</returns>
        [HttpPost("Create")]
        public async Task<ActionResult> Create(int idPublicacion, [FromBody] ComentarioDTO comentario)
        {
            try
            {
                // obtenemos el usuario autenticado (mediante token)
                Usuario authUser = await Fun.GetAuthUser(Repository.GetContext(), User);

                if (authUser == null)
                {
                    return NotFound("El usuario autenticado no existe o no se logró obtener su información");
                }

                // obtenemos la publicacion
                PublicacionDTO publicacion = (await RepositoryPublicacion.GetAsync(n => n.Id == idPublicacion)).FirstOrDefault();

                if (publicacion == null)
                {
                    return NotFound("La publicación no ha sido encontrada");
                }

                // creamos un objeto de la tabla Publicacion desde PublicacionDTO
                Comentario nuevo = Repository.GetMapper().Map<Comentario>(comentario);
                nuevo.IdPublicacion = publicacion.Id;
                nuevo.IdUsuario = authUser.Id;
                nuevo.IdPublicacion = idPublicacion;
                nuevo.FechaAlta = DateTime.Now;

                // si no esta duplicado creamos el nuevo usuario y con commit guardamos cambios
                await Repository.CreateAsync(nuevo);
                await Repository.Commit();

                // retornamos el usuario con su nuevo id
                return new CreatedAtRouteResult("GetComentario", new
                {
                    id = nuevo.Id
                }, Repository.GetMapper().Map<ComentarioDTO>(nuevo));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Crea un comentario a otro comentario (de igual forma se pude utilizar el Create normal para realizar esto)
        /// </summary>
        /// <param name="idPublicacion">Id de la publicación que pertenecerá el comentario</param>
        /// <param name="idComentario">Id del comentario padre</param>
        /// <param name="comentario">Comentario creado</param>
        /// <returns>El comentario creado</returns>
        [HttpPost("CreateComentarioFromComentario")]
        public async Task<ActionResult> CreateComentarioFromComentario(int idPublicacion, int idComentario, [FromBody] ComentarioDTO comentario)
        {
            try
            {
                // obtenemos el usuario autenticado (mediante token)
                Usuario authUser = await Fun.GetAuthUser(Repository.GetContext(), User);

                if (authUser == null)
                {
                    return NotFound("El usuario autenticado no existe o no se logró obtener su información");
                }

                // obtenemos la publicacion
                PublicacionDTO publicacion = (await RepositoryPublicacion.GetAsync(n => n.Id == idPublicacion)).FirstOrDefault();

                if (publicacion == null)
                {
                    return NotFound("La publicación no ha sido encontrada");
                }

                // creamos un objeto de la tabla Publicacion desde PublicacionDTO
                Comentario nuevo = Repository.GetMapper().Map<Comentario>(comentario);
                nuevo.IdComentarioPadre = idComentario;
                nuevo.IdPublicacion = publicacion.Id;
                nuevo.IdUsuario = authUser.Id;
                nuevo.IdPublicacion = idPublicacion;
                nuevo.FechaAlta = DateTime.Now;

                // si no esta duplicado creamos el nuevo usuario y con commit guardamos cambios
                await Repository.CreateAsync(nuevo);
                await Repository.Commit();

                // retornamos el usuario con su nuevo id
                return new CreatedAtRouteResult("GetComentario", new
                {
                    id = nuevo.Id
                }, Repository.GetMapper().Map<ComentarioDTO>(nuevo));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene un comentario específico
        /// </summary>
        /// <param name="id">Id del comentario a obtener</param>
        /// <returns>Comentario</returns>
        [HttpGet("{id:int}", Name = "GetComentario")]
        [AllowAnonymous]
        public async Task<ActionResult<ComentarioDTO>> Get(int id)
        {
            try
            {
                ComentarioDTO nodo = (await Repository.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (nodo == null)
                {
                    return NotFound("Comentario no encontrado"); // retorna un codigo de error 404
                }

                UsuarioDTO usuarioComentario = (await RepositoryUsuario.GetAsync(n => n.Id == nodo.IdUsuario)).FirstOrDefault();

                if (usuarioComentario != null)
                {
                    nodo.NombreCompleto = $"{usuarioComentario.Nombre} {usuarioComentario.Apellido}";
                    nodo.RutaImagenAvatar = usuarioComentario.RutaImagenAvatar;
                }

                return nodo;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un comentario
        /// </summary>
        /// <param name="id">Id del comentario</param>
        /// <returns>Estado de success 204</returns>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                ComentarioDTO dto = (await Repository.GetAsync(n => n.Id == id)).FirstOrDefault();
                Comentario nodo = Repository.GetMapper().Map<Comentario>(dto);

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
        /// Obtiene los comentarios de una publicación
        /// </summary>
        /// <param name="id">Id de la publicación</param>
        /// <returns>Lista de comentarios de la publicación indicada</returns>
        [HttpGet("GetComentariosByIdPublicacion/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ComentarioDTO>>> GetComentariosByIdPublicacion(int id)
        {
            try
            {
                var comentarios = await Repository.GetAsync(n => n.IdPublicacion == id && n.IdComentarioPadre == 0);

                foreach (ComentarioDTO comentario in comentarios)
                {
                    await LlenarComentarios(comentario);
                }

                return comentarios;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene los comentarios de un comentario
        /// </summary>
        /// <param name="id">Id del comentario</param>
        /// <returns>Lista de comentarios del comentario indicado</returns>
        [HttpGet("GetComentariosByIdComentario/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ComentarioDTO>>> GetComentariosByIdComentario(int id)
        {
            try
            {
                var comentarios = await Repository.GetAsync(n => n.IdComentarioPadre == id);

                foreach (ComentarioDTO comentario in comentarios)
                {
                    await LlenarComentarios(comentario);
                }

                return comentarios;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task LlenarComentarios(ComentarioDTO comentario)
        {
            UsuarioDTO usuarioComentario = (await RepositoryUsuario.GetAsync(n => n.Id == comentario.IdUsuario)).FirstOrDefault();

            if (usuarioComentario != null)
            {
                comentario.NombreCompleto = $"{usuarioComentario.Nombre} {usuarioComentario.Apellido}";
                comentario.RutaImagenAvatar = usuarioComentario.RutaImagenAvatar;
            }

            comentario.Comentarios = await Repository.GetAsync(n => n.IdComentarioPadre == comentario.Id);

            foreach (ComentarioDTO com in comentario.Comentarios)
            {
                await LlenarComentarios(com);
            }
        }
    }
}
