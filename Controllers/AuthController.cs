using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
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
using Xchangez.Clases;
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // esto indica que necesita autorización por token
    public class AuthController : ControllerBase
    {
        // aqui es donde se guardan los archivos de un usuario, {0} es el id del usuario
        private const string RUTA_ARCHIVOS_MULTIMEDIA = "multimedia/usuarios/{0}";

        private readonly IRepository<Usuario, UsuarioDTO> Repository;
        private readonly IConfiguration Configuration;
        private readonly IFile SaveFile;

        public AuthController(IConfiguration configuration, IRepository<Usuario, UsuarioDTO> repository, IFile saveFile)
        {
            Repository = repository;
            Configuration = configuration;

            SaveFile = saveFile;
        }

        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        /// <returns>Lista de usuarios</returns>
        [HttpGet("Usuarios")]
        [AllowAnonymous]
        public async Task<ActionResult<List<UsuarioDTO>>> GetUsuarios()
        {
            try
            {
                var users = await Repository.GetAsync();

                foreach (UsuarioDTO usuario in users)
                {
                    usuario.Password = string.Empty;
                    usuario.Valoracion = await Fun.GetAverage(Repository.GetContext(), usuario.Id);
                }

                return users;
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
        [AllowAnonymous]
        public async Task<ActionResult<UsuarioDTO>> GetUsuario(int id)
        {
            try
            {
                UsuarioDTO usuario = (await Repository.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (usuario == null)
                {
                    return NotFound("El usuario no existe");
                }

                usuario.Password = string.Empty;
                usuario.Valoracion = await Fun.GetAverage(Repository.GetContext(), usuario.Id);
                usuario.CantidadSeguidores = await Fun.GetCantidadSeguidores(Repository.GetContext(), usuario.Id);
                usuario.CantidadSeguidos = await Fun.GetCantidadSeguidos(Repository.GetContext(), usuario.Id);

                return usuario;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Crea un usuario en la base de datos
        /// </summary>
        /// <param name="usuario">DTO del usuario a crear</param>
        /// <returns>El usuario creado junto a su ID</returns>
        [HttpPost("Create")]
        [AllowAnonymous] // permite la consulta de la accion aunque el controlador pida token
        public async Task<ActionResult> Create([FromBody] UsuarioDTO usuario)
        {
            try
            {
                // creamos un objeto de la tabla Usuario desde UsuarioDTO
                Usuario nuevo = new Usuario()
                {
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Correo = usuario.Correo,
                    Password = usuario.Password,
                    Nick= string.Empty,
                    FechaNacimiento = usuario.FechaNacimiento,
                    Valoracion = usuario.Valoracion,
                    RutaImagenAvatar = usuario.RutaImagenAvatar,
                    RutaImagenPortada = usuario.RutaImagenPortada,
                    EsPrivado = usuario.EsPrivado
                };

                // verificamos si esta duplicado
                UsuarioDTO usuarioDuplicado = (await Repository.GetAsync(n => n.Correo == usuario.Correo)).FirstOrDefault();

                if (usuarioDuplicado != null)
                {
                    return BadRequest("El correo proporcionado ya esta en uso");
                }

                // si no esta duplicado creamos el nuevo usuario y con commit guardamos cambios
                await Repository.CreateAsync(nuevo);
                await Repository.Commit();

                // retornamos el usuario con su nuevo id
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
        [AllowAnonymous] // permite la consulta de la accion aunque el controlador pida token
        public async Task<ActionResult<UserToken>> Login([FromBody] UserInfo usuario)
        {
            try
            {
                UsuarioDTO nodo = (await Repository.GetAsync(n => n.Correo.Equals(usuario.Correo))).FirstOrDefault();

                if (nodo == null || !nodo.Password.Equals(usuario.Password))
                {
                    return BadRequest("El usuario y/o contraseña no coinciden");
                }

                // si las credenciales fueron correctas regresamos su token
                return BuildToken(nodo, DateTime.Now.AddMinutes(Constantes.C_AUTH_EXPIRACION_TOKEN));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza la imagen de avatar del usuario autenticado
        /// </summary>
        /// <param name="tipoImagen">Es el tipo de imagen a guardar (1.- avatar, 2.- portada)</param>
        /// <param name="imagen">Imagen para actualizar</param>
        /// <returns>204</returns>
        [HttpPost("UpdateAvatarImage")]
        public async Task<ActionResult> UpdateImage([FromForm] int tipoImagen, IFormFile imagen)
        {
            try
            {
                // obtenemos el usuario autenticado (mediante token)
                Usuario authUser = await Fun.GetAuthUser(Repository.GetContext(), User);

                if (authUser == null)
                {
                    return NotFound("El usuario autenticado no existe o no se logró obtener su información");
                }

                await GuardarMultimedia(authUser, imagen, tipoImagen);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Establece si el usuario autenticado es privado
        /// </summary>
        /// <returns>Estado de success 204</returns>
        [HttpPost("EstablecerPrivado/{esPrivado:bool?}")]
        public async Task<ActionResult> Put(bool esPrivado = true)
        {
            try
            {
                Usuario usuario = await Fun.GetAuthUser(Repository.GetContext(), User);

                if (usuario == null)
                {
                    return NotFound("El usuario no esta autenticado");
                }

                usuario.EsPrivado = esPrivado;

                // actualizamos y guardamos
                Repository.Update(usuario);
                await Repository.Commit();

                // retornamos 0 contenido pero sastifactorio
                return NoContent();
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

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Constantes.JWT_SECRETKEY_VALUE));
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

        /// <summary>
        /// Guarda un archivo adjunto a un usuario
        /// </summary>
        /// <param name="nodo">usuario</param>
        /// <param name="archivo">archivo multimedia</param>
        /// <param name="tipoImagen">Es el tipo de imagen a guardar (1.- avatar, 2.- portada)</param>
        /// <returns>No retorna nada relevante</returns>
        private async Task GuardarMultimedia(Usuario nodo, IFormFile archivo, int tipoImagen)
        {
            // se valida si adjunto multimedia (imagenes, videos, etc.)
            if (archivo == null)
            {
                return;
            }

            try
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    // copiamos el contenido a memory
                    await archivo.CopyToAsync(memory);

                    // obtenemos los bytes del archivo
                    byte[] contenido = memory.ToArray();

                    // obtenemos su nombre
                    string nombre = Path.GetFileNameWithoutExtension(archivo.FileName);

                    // se obtiene solo su extension
                    string extension = Path.GetExtension(archivo.FileName);

                    // se construye el directorio donde se guardara
                    string directorio = string.Format(RUTA_ARCHIVOS_MULTIMEDIA, nodo.Id);

                    // se guarda el archivo y se obtiene su ruta
                    string ruta = await SaveFile.SaveAsync(contenido, nombre, extension, directorio, archivo.ContentType);

                    // actualizamos usuario
                    if (tipoImagen == 1)
                    {
                        nodo.RutaImagenAvatar = ruta;
                    } 
                    else if (tipoImagen == 2)
                    {
                        nodo.RutaImagenPortada = ruta;
                    }

                    // verificamos que no exista igual
                    Repository.Update(nodo);
                    await Repository.Commit();

                    memory.Close();
                    await memory.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
