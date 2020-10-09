using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Xchangez.Authentication;
using Xchangez.DTOs;
using Xchangez.Interfaces;
using Xchangez.Models;

namespace Xchangez.Controllers
{
    /// <summary>
    /// Controlador para manipular todo lo relacionado con usuarios (tokens, consultas, etc.)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AuthController : ControllerBase
    {
        private readonly IRepository<Usuario, UsuarioDTO> Repository;
        private readonly IConfiguration Configuration;

        public AuthController(IConfiguration configuration, IRepository<Usuario, UsuarioDTO> repository)
        {
            Repository = repository;
            Configuration = configuration;
        }

        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        /// <returns>Lista de usuarios</returns>
        [HttpGet("Usuarios")]
        public async Task<ActionResult<List<UsuarioDTO>>> GetUsuarios()
        {
            try
            {
                return await Repository.GetAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene un usuario en especifico
        /// </summary>
        /// <param name="id">Id del usuario que se quiere obtener</param>
        /// <returns>Usuario</returns>
        [HttpGet("{id:int}", Name = "GetUsuario")]
        public async Task<ActionResult<UsuarioDTO>> GetUsuario(int id)
        {
            try
            {
                var list = await Repository.GetAsync(n => n.Id == id);

                return Repository.GetMapper().Map<UsuarioDTO>(list.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Crea un usuario en la base de datos
        /// </summary>
        /// <param name="usuario">Datos del usuario a crear</param>
        /// <returns>El usuario creado</returns>
        [HttpPost("Create")]
        [AllowAnonymous]
        public async Task<ActionResult> Create([FromBody] UsuarioDTO usuario)
        {
            try
            {
                Usuario nuevo = new Usuario()
                {
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Correo = usuario.Correo,
                    Password = usuario.Password,
                    FechaNacimiento = usuario.FechaNacimiento
                };

                UsuarioDTO usuarioDuplicado = (await Repository.GetAsync(n => n.Correo == usuario.Correo)).FirstOrDefault();

                if (usuarioDuplicado != null)
                {
                    return BadRequest("El correo proporcionado ya esta en uso");
                }

                await Repository.CreateAsync(nuevo);
                await Repository.Commit();

                return new CreatedAtRouteResult("GetUsuario", new
                {
                    id = nuevo.Id
                }, Repository.GetMapper().Map<UsuarioDTO>(nuevo));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Verifica coincidencias del usuario
        /// </summary>
        /// <param name="usuario">Usuario que se logeara</param>
        /// <returns>Token con la información del usuario</returns>
        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<ActionResult<UserToken>> Login([FromBody] UserInfo usuario)
        {
            try
            {
                UsuarioDTO nodo = (await Repository.GetAsync(n => n.Correo.Equals(usuario.Correo))).FirstOrDefault();

                if (nodo == null || !nodo.Password.Equals(usuario.Password))
                {
                    return BadRequest("El usuario y/o contraseña no coinciden");
                }

                return BuildToken(nodo, DateTime.Now.AddMinutes(5));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Construye un token de seguridad
        /// </summary>
        /// <param name="usuario">Usuario para obtener la informacion</param>
        /// <param name="fechaExpiracion">Fecha en que expirará el token</param>
        /// <returns>objeto anonimo con el token y su fecha de expiracion</returns>
        private ActionResult<UserToken> BuildToken(UsuarioDTO usuario, DateTime fechaExpiracion)
        {
            Claim[] claims = new[]
            {
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName, Convert.ToString(usuario.Id)),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, usuario.Correo),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration[Constantes.JWT_SECRETKEY_NAME]));
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: fechaExpiracion,
                signingCredentials: credentials);

            return Ok(new UserToken()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = fechaExpiracion
            });
        }
    }
}
