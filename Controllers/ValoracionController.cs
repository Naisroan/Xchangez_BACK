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
    public class ValoracionController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        private readonly IRepository<Valoracion, ValoracionDTO> Repository;
        private readonly IRepository<Usuario, UsuarioDTO> RepositoryUsuario;

        public ValoracionController(IConfiguration configuration, IRepository<Valoracion, ValoracionDTO> repository,
            IRepository<Usuario, UsuarioDTO> repositoryUsuario)
        {
            Configuration = configuration;
            Repository = repository;
            RepositoryUsuario = repositoryUsuario;
        }

        /// <summary>
        /// Obtiene una valoracion en especifico
        /// </summary>
        /// <param name="id">Id de la valoración a obtener</param>
        /// <returns>Valoracion</returns>
        [HttpGet("{id:int}", Name = "GetValoracion")]
        [AllowAnonymous]
        public async Task<ActionResult<ValoracionDTO>> Get(int id)
        {
            try
            {
                ValoracionDTO nodo = (await Repository.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (nodo == null)
                {
                    return NotFound("Valoracion no encontrada"); // retorna un codigo de error 404
                }

                return nodo;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene las valoraciones de un usuario (es decir, las valoraciones que le han hecho a un usuario)
        /// </summary>
        /// <param name="id">Id del usuario</param>
        /// <returns>Lista valoraciones hecha a un usuario</returns>
        [HttpGet("GetValoracionesByIdUsuarioValorado/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ValoracionDTO>>> GetValoracionesByIdUsuarioValorado(int id)
        {
            try
            {
                // obtenemos al usuario valorado
                UsuarioDTO usuario = (await RepositoryUsuario.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (usuario == null)
                {
                    return NotFound("El usuario no ha sido encontrado");
                }

                var valoraciones = await Repository.GetAsync(n => n.IdUsuarioValorado == id);
                await LlenarInformacionExtra(valoraciones);

                return valoraciones;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene las valoraciones que ha hecho un usuario
        /// </summary>
        /// <param name="id">Id del usuario</param>
        /// <returns>Lista de valoraciones realizada a un usuario</returns>
        [HttpGet("GetValoracionesByIdUsuario/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ValoracionDTO>>> GetValoracionesByIdUsuario(int id)
        {
            try
            {
                // obtenemos al usuario valorado
                UsuarioDTO usuario = (await RepositoryUsuario.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (usuario == null)
                {
                    return NotFound("El usuario no ha sido encontrado");
                }

                var valoraciones = await Repository.GetAsync(n => n.IdUsuario == id);
                await LlenarInformacionExtra(valoraciones);

                return valoraciones;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el promedio de valoración de un usuario
        /// </summary>
        /// <param name="id">Id del usuario</param>
        /// <returns>Promedio de valoración</returns>
        [HttpGet("GetPromedioByIdUsuario/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<double>> GetPromedioByIdUsuario(int id)
        {
            try
            {
                // obtenemos al usuario valorado
                UsuarioDTO usuario = (await RepositoryUsuario.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (usuario == null)
                {
                    return NotFound("El usuario no ha sido encontrado");
                }

                var valoraciones = await Repository.GetAsync(n => n.IdUsuarioValorado == id);

                return valoraciones.Count != 0 ? valoraciones.Select(n => n.Cantidad).Average() : 0.0;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Registra una valoracion
        /// </summary>
        /// <param name="idUsuario">Id del usuario que se ha valorado</param>
        /// <param name="valoracion">Valoracion realizada</param>
        /// <returns>La valoracion</returns>
        [HttpPost("Create")]
        public async Task<ActionResult> Create(int idUsuario, [FromBody] ValoracionDTO valoracion)
        {
            try
            {
                // obtenemos el usuario autenticado (mediante token)
                Usuario authUser = await Fun.GetAuthUser(Repository.GetContext(), User);

                if (authUser == null)
                {
                    return NotFound("El usuario autenticado no existe o no se logró obtener su información");
                }

                // obtenemos al usuario valorado
                UsuarioDTO usuario = (await RepositoryUsuario.GetAsync(n => n.Id == idUsuario)).FirstOrDefault();

                if (usuario == null)
                {
                    return NotFound("El usuario a valorar no existe");
                }

                // creamos un objeto de la tabla Valoracion desde ValoracionDTO
                Valoracion nuevo = Repository.GetMapper().Map<Valoracion>(valoracion);
                nuevo.IdUsuarioValorado = idUsuario;
                nuevo.IdUsuario = authUser.Id;
                nuevo.FechaAlta = DateTime.Now;

                // si no esta duplicado creamos el nuevo usuario y con commit guardamos cambios
                await Repository.CreateAsync(nuevo);
                await Repository.Commit();

                // retornamos el usuario con su nuevo id
                return new CreatedAtRouteResult("GetValoracion", new
                {
                    id = nuevo.Id
                }, Repository.GetMapper().Map<ValoracionDTO>(nuevo));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Verifica si el usuario autenticado ya a valorado a un usuario
        /// </summary>
        /// <param name="id">Id del usuario que se verificará si ya fue valorado o no</param>
        /// <returns>La valoracion</returns>
        [HttpGet("YaFueValorado/{id:int}")]
        public async Task<ActionResult<bool>> GetYaFueValorado(int id)
        {
            try
            {
                // obtenemos el usuario autenticado (mediante token)
                Usuario authUser = await Fun.GetAuthUser(Repository.GetContext(), User);

                if (authUser == null)
                {
                    return NotFound("El usuario autenticado no existe o no se logró obtener su información");
                }

                // obtenemos al usuario valorado
                UsuarioDTO usuario = (await RepositoryUsuario.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (usuario == null)
                {
                    return NotFound("El usuario a valorar no existe");
                }

                var valoraciones = await Repository.GetAsync(n => n.IdUsuario == authUser.Id && n.IdUsuarioValorado == usuario.Id);

                return valoraciones.Count > 0;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task LlenarInformacionExtra(List<ValoracionDTO> valoraciones)
        {
            if (valoraciones == null || valoraciones.Count <= 0)
            {
                return;
            }

            foreach (ValoracionDTO valoracion in valoraciones)
            {
                UsuarioDTO usuario = (await RepositoryUsuario.GetAsync(n => n.Id == valoracion.IdUsuario)).FirstOrDefault();

                if (usuario != null)
                {
                    valoracion.NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}";
                    valoracion.RutaImagenAvatar = usuario.RutaImagenAvatar;
                }
            }
        }
    }
}
