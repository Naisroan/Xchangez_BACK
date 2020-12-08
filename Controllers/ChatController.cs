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
using Xchangez.Hubs;
using Xchangez.Interfaces;
using Xchangez.Models;

namespace Xchangez.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // esto indica que necesita autorización por token
    public class ChatController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        private readonly IRepository<Mensaje, MensajeDTO> Repository;
        private readonly IRepository<Usuario, UsuarioDTO> RepositoryUsuario;

        public ChatController(IConfiguration configuration, IRepository<Mensaje, MensajeDTO> repository,
            IRepository<Usuario, UsuarioDTO> repositoryUsuario)
        {
            Configuration = configuration;
            Repository = repository;
            RepositoryUsuario = repositoryUsuario;
        }

        /// <summary>
        /// Registra un mensaje al usuario autenticado y lo envia a un usuario en especifico
        /// </summary>
        /// <param name="idUsuario">Id del usuario al que se desea mandar un mensaje</param>
        /// <param name="mensaje">Estructura del mensaje</param>
        /// <returns>La valoracion</returns>
        [HttpPost("Create")]
        public async Task<ActionResult> Create(int idUsuario, [FromBody] MensajeDTO mensaje)
        {
            try
            {
                // obtenemos el usuario autenticado (mediante token)
                Usuario authUser = await Fun.GetAuthUser(Repository.GetContext(), User);

                if (authUser == null)
                {
                    return NotFound("El usuario autenticado no existe o no se logró obtener su información");
                }

                // obtenemos el usuario al que se desea mandar el mensaje
                UsuarioDTO usuario = (await RepositoryUsuario.GetAsync(n => n.Id == idUsuario)).FirstOrDefault();

                if (usuario == null)
                {
                    return NotFound("El usuario con el que se desea comunicar no existe");
                }

                // creamos un objeto
                Mensaje nuevo = Repository.GetMapper().Map<Mensaje>(mensaje);
                nuevo.IdUsuario = authUser.Id;
                nuevo.IdGrupo = idUsuario;
                nuevo.FechaAlta = DateTime.Now;

                // si no esta duplicado creamos el nuevo usuario y con commit guardamos cambios
                await Repository.CreateAsync(nuevo);
                await Repository.Commit();

                // retornamos el usuario con su nuevo id
                return new CreatedAtRouteResult("GetMensaje", new
                {
                    id = nuevo.Id
                }, Repository.GetMapper().Map<MensajeDTO>(nuevo));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Registra un mensaje a un usuario anonimo y lo envia a un usuario en especifico
        /// </summary>
        /// <param name="idAnonimo">Id del usuario anónimo</param>
        /// <param name="mensaje">Estructura del mensaje</param>
        /// <returns>La valoracion</returns>
        [HttpPost("CreateAnonimo")]
        public async Task<ActionResult> CreateAnonimo(int idAnonimo, [FromBody] MensajeDTO mensaje)
        {
            try
            {
                // obtenemos el usuario autenticado (mediante token)
                Usuario authUser = await Fun.GetAuthUser(Repository.GetContext(), User);

                if (authUser == null)
                {
                    return NotFound("El usuario autenticado no existe o no se logró obtener su información");
                }

                // creamos un objeto
                Mensaje nuevo = Repository.GetMapper().Map<Mensaje>(mensaje);
                nuevo.IdUsuario = authUser.Id;
                nuevo.IdGrupo = idAnonimo;
                nuevo.FechaAlta = DateTime.Now;

                // si no esta duplicado creamos el nuevo usuario y con commit guardamos cambios
                // await Repository.CreateAsync(nuevo);
                // await Repository.Commit();

                // retornamos el usuario con su nuevo id
                return new CreatedAtRouteResult("GetMensaje", new
                {
                    id = nuevo.Id
                }, Repository.GetMapper().Map<MensajeDTO>(nuevo));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene un mensaje en específico
        /// </summary>
        /// <param name="id">Id del mensaje a obtener</param>
        /// <returns>Valoracion</returns>
        [HttpGet("{id:int}", Name = "GetMensaje")]
        [AllowAnonymous]
        public async Task<ActionResult<MensajeDTO>> Get(int id)
        {
            try
            {
                MensajeDTO nodo = (await Repository.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (nodo == null)
                {
                    return NotFound("Mensaje no encontrado"); // retorna un codigo de error 404
                }

                return nodo;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
