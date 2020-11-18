using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xchangez.Interfaces;
using Xchangez.Models;

namespace Xchangez.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // esto indica que necesita autorización por token
    public class PublicacionController : ControllerBase
    {
        // aqui es donde se guardan los archivos de una publicación, {0} es el id de la publicacion
        private const string RUTA_ARCHIVOS_MULTIMEDIA = "multimedia/{0}";

        private readonly IRepository<Publicacion, PublicacionDTO> Repository;
        private readonly IRepository<Comentario, ComentarioDTO> RepositoryComentarios;
        private readonly IRepository<Multimedia, MultimediaDTO> RepositoryMultimedia;
        private readonly IFile SaveFile;
        private readonly IConfiguration Configuration;

        public PublicacionController(IConfiguration configuration, IRepository<Publicacion, PublicacionDTO> repository, 
            IRepository<Comentario, ComentarioDTO> repositoryComentarios,
            IRepository<Multimedia, MultimediaDTO> repositoryMultimedia,
            IFile saveFile)
        {
            Configuration = configuration;

            Repository = repository;
            RepositoryComentarios = repositoryComentarios;
            RepositoryMultimedia = repositoryMultimedia;

            SaveFile = saveFile;
        }

        /// <summary>
        /// Obtiene todas las publicaciones
        /// </summary>
        /// <returns>Lista de publicaciones</returns>
        [HttpGet("Publicaciones")]
        public async Task<ActionResult<List<PublicacionDTO>>> Get()
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
        /// Obtiene una publicacion específica
        /// </summary>
        /// <param name="id">Id de la publicación que se quiere obtener</param>
        /// <returns>Publicación</returns>
        [HttpGet("{id:int}", Name = "GetPublicacion")]
        public async Task<ActionResult<PublicacionDTO>> Get(int id)
        {
            try
            {
                PublicacionDTO nodo = (await Repository.GetAsync(n => n.Id == id)).FirstOrDefault();
                
                if (nodo == null)
                {
                    return NotFound("Publicación no encontrada"); // retorna un codigo de error 404
                }

                return nodo;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Crea una publicación en la base de datos sin archivos multimedia en base al usuario autenticado por token
        /// </summary>
        /// <param name="nodo">Publicación a crear</param>
        /// <returns>La publicacion creada junto a su ID</returns>
        [HttpPost("Create")]
        public async Task<ActionResult> Create([FromBody] PublicacionDTO nodo)
        {
            try
            {
                // obtenemos el usuario autenticado (mediante token)
                Usuario authUser = await Fun.GetAuthUser(Repository.GetContext(), User);

                if (authUser == null)
                {
                    return NotFound("El usuario autenticado no existe o no se logró obtener su información");
                }

                // creamos un objeto de la tabla Publicacion desde PublicacionDTO
                Publicacion nuevo = Repository.GetMapper().Map<Publicacion>(nodo);
                nuevo.IdUsuario = authUser.Id;
                nuevo.FechaAlta = DateTime.Now;
                nuevo.FechaModificacion = DateTime.Now;

                // creamos y guardamos
                await Repository.CreateAsync(nuevo);
                await Repository.Commit();

                // retornamos la publicacion con su nuevo id
                return new CreatedAtRouteResult("GetPublicacion", new
                {
                    id = nuevo.Id
                }, Repository.GetMapper().Map<PublicacionDTO>(nuevo));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Crea una publicación en la base de datos con archivos multimedia en base al usuario autenticado por token
        /// </summary>
        /// <param name="nodo">Publicación a crear</param>
        /// <param name="multimedias">Lista de imagenes o videos</param>
        /// <returns>La publicacion creada junto a su ID</returns>
        [HttpPost("CreateWithFiles")]
        public async Task<ActionResult> Create([FromForm] PublicacionDTO nodo, List<IFormFile> multimedias)
        {
            try
            {
                // obtenemos el usuario autenticado (mediante token)
                Usuario authUser = await Fun.GetAuthUser(Repository.GetContext(), User);

                if (authUser == null)
                {
                    return NotFound("El usuario autenticado no existe o no se logró obtener su información");
                }

                // creamos un objeto de la tabla Publicacion desde PublicacionDTO
                Publicacion nuevo = Repository.GetMapper().Map<Publicacion>(nodo);
                nuevo.IdUsuario = authUser.Id;
                nuevo.FechaAlta = DateTime.Now;
                nuevo.FechaModificacion = DateTime.Now;

                // creamos y guardamos
                await Repository.CreateAsync(nuevo);
                await Repository.Commit();

                // guardamos los archivos si es que adjunto
                await GuardarMultimedias(nuevo, multimedias);

                // retornamos el usuario con su nuevo id
                return new CreatedAtRouteResult("GetPublicacion", new
                {
                    id = nuevo.Id
                }, Repository.GetMapper().Map<PublicacionDTO>(nuevo));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Adjunta un archivo multimedia a una publicación
        /// </summary>
        /// <param name="idPublicacion">Id de la publicación a crear un archivo multimedia</param>
        /// <param name="archivo">Archivo multimedia</param>
        /// <returns>La publicacion a la que se le adjunto el archivo</returns>
        [HttpPost("AddFile")]
        public async Task<ActionResult> AddFile([FromForm] int idPublicacion, IFormFile archivo)
        {
            try
            {
                // obtenemos el usuario autenticado (mediante token)
                Usuario authUser = await Fun.GetAuthUser(Repository.GetContext(), User);

                if (authUser == null)
                {
                    return NotFound("El usuario autenticado no existe o no se logró obtener su información");
                }

                // se consulta a la bd el nodo a editar y valida que existe
                PublicacionDTO valid = (await Repository.GetAsync(n => n.Id == idPublicacion)).FirstOrDefault();

                if (valid == null)
                {
                    return NotFound("No se encontró la publicación a modificar"); // retorna un codigo de error 404
                }

                Publicacion publicacion = Repository.GetMapper().Map<Publicacion>(valid);
                publicacion.FechaModificacion = DateTime.Now;

                // guardamos el archivo adjuntado
                await GuardarMultimedia(publicacion, archivo);

                // actualizamos y guardamos
                Repository.Update(publicacion);
                await Repository.Commit();

                // retornamos la publicacion con su nuevo id
                return new CreatedAtRouteResult("GetPublicacion", new
                {
                    id = publicacion.Id
                }, Repository.GetMapper().Map<PublicacionDTO>(publicacion));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Adjunta uno o mas archivos multimedias a una publicación
        /// </summary>
        /// <param name="idPublicacion">Id de la publicación a crear un archivo multimedia</param>
        /// <param name="multimedias">Archivos multimedias</param>
        /// <returns>La publicacion a la que se le adjunto el archivo</returns>
        [HttpPost("AddFiles")]
        public async Task<ActionResult> AddFiles([FromForm] int idPublicacion, List<IFormFile> multimedias)
        {
            try
            {
                // obtenemos el usuario autenticado (mediante token)
                Usuario authUser = await Fun.GetAuthUser(Repository.GetContext(), User);

                if (authUser == null)
                {
                    return NotFound("El usuario autenticado no existe o no se logró obtener su información");
                }

                // se consulta a la bd el nodo a editar y valida que existe
                PublicacionDTO valid = (await Repository.GetAsync(n => n.Id == idPublicacion)).FirstOrDefault();

                if (valid == null)
                {
                    return NotFound("No se encontró la publicación a adjuntar el archivo"); // retorna un codigo de error 404
                }

                Publicacion publicacion = Repository.GetMapper().Map<Publicacion>(valid);
                publicacion.FechaModificacion = DateTime.Now;

                // guardamos el archivo adjuntado
                await GuardarMultimedias(publicacion, multimedias);

                // actualizamos y guardamos
                Repository.Update(publicacion);
                await Repository.Commit();

                // retornamos la publicacion con su nuevo id
                return new CreatedAtRouteResult("GetPublicacion", new
                {
                    id = publicacion.Id
                }, Repository.GetMapper().Map<PublicacionDTO>(publicacion));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Modifica una publicación
        /// </summary>
        /// <param name="id">Id de la publicación</param>
        /// <param name="nodo">Publicacion en si que se modificará</param>
        /// <returns>Estado de success 204</returns>
        [HttpPost]
        public async Task<ActionResult> Put(int id, [FromForm] PublicacionDTO nodo)
        {
            try
            {
                // se consulta a la bd el nodo a editar y valida que existe
                var nodoDB = (await Repository.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (nodoDB == null)
                {
                    return NotFound("No se encontró la publicación a modificar"); // retorna un codigo de error 404
                }

                // se obtiene 'edit' que es el nodo DTO mapeado a su clase abstracta
                Publicacion publicacion = Repository.GetMapper().Map<Publicacion>(nodo);

                publicacion.Id = id;
                publicacion.IdUsuario = nodoDB.IdUsuario;
                publicacion.FechaAlta = nodoDB.FechaAlta;
                publicacion.FechaModificacion = DateTime.Now;

                // actualizamos y guardamos
                Repository.Update(publicacion);
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
        /// Modifica una publicación con archivos multimedia
        /// </summary>
        /// <param name="id">Id de la publicación</param>
        /// <param name="nodo">Publicacion en si que se modificará</param>
        /// <param name="multimedias">Lista de imagenes o videos</param>
        /// <returns>Estado de success 204</returns>
        [HttpPost("UpdateWithFiles")]
        public async Task<ActionResult> Put([FromForm] int id, PublicacionDTO nodo, List<IFormFile> multimedias)
        {
            try
            {
                // se consulta a la bd el nodo a editar y valida que existe
                var nodoDB = (await Repository.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (nodoDB == null)
                {
                    return NotFound("No se encontró la publicación a modificar"); // retorna un codigo de error 404
                }

                // se obtiene 'edit' que es el nodo DTO mapeado a su clase abstracta
                Publicacion publicacion = Repository.GetMapper().Map<Publicacion>(nodo);

                publicacion.Id = id;
                publicacion.IdUsuario = nodoDB.IdUsuario;
                publicacion.FechaAlta = nodoDB.FechaAlta;
                publicacion.FechaModificacion = DateTime.Now;

                // actualizamos y guardamos
                Repository.Update(publicacion);
                await Repository.Commit();

                // guardamos los archivos si es que adjunto
                await GuardarMultimedias(publicacion, multimedias);

                // retornamos 0 contenido pero sastifactorio
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Elimina una publicacion en base a su id
        /// </summary>
        /// <param name="id">Id de la publicación</param>
        /// <returns>Estado de success 204</returns>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                PublicacionDTO dto = (await Repository.GetAsync(n => n.Id == id)).FirstOrDefault();
                Publicacion nodo = Repository.GetMapper().Map<Publicacion>(dto);

                // obtenemos comentarios de la publicación
                List<ComentarioDTO> comentarioDTOs = (await RepositoryComentarios.GetAsync(n => n.IdPublicacion == id));

                // los eliminamos
                if (comentarioDTOs != null && comentarioDTOs.Count > 0)
                {
                    foreach (ComentarioDTO comentarioDTO in comentarioDTOs)
                    {
                        Comentario comentario = RepositoryComentarios.GetMapper().Map<Comentario>(comentarioDTO);

                        RepositoryComentarios.Delete(comentario);
                        await RepositoryComentarios.Commit();
                    }
                }

                // obtenemos archivos de la publicación
                List<MultimediaDTO> multimediasDTOs = (await RepositoryMultimedia.GetAsync(n => n.IdPublicacion == id));

                // los eliminamos
                if (multimediasDTOs != null && multimediasDTOs.Count > 0)
                {
                    foreach (MultimediaDTO multimediaDTO in multimediasDTOs)
                    {
                        Multimedia multimedia = RepositoryMultimedia.GetMapper().Map<Multimedia>(multimediaDTO);
                        string directorio = string.Format(RUTA_ARCHIVOS_MULTIMEDIA, nodo.Id);

                        await SaveFile.Delete(directorio, multimediaDTO.Ruta);

                        RepositoryMultimedia.Delete(multimedia);
                        await RepositoryMultimedia.Commit();
                    }
                }

                // finalmente borramos la publicación
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
        /// Obtiene todas los archivos de una publicacion
        /// </summary>
        /// <param name="id">Id de la publicación de donde se quiere obtener los archivos</param>
        /// <returns>Lista de archivos de una publicacion</returns>
        [HttpGet("GetFiles/{id:int}")]
        public async Task<ActionResult<List<MultimediaDTO>>> GetFiles(int id)
        {
            try
            {
                return await RepositoryMultimedia.GetAsync(n => n.IdPublicacion == id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un archivo multimedia
        /// </summary>
        /// <param name="id">Id del archivo multimedia</param>
        /// <returns>Estado de success 204</returns>
        [HttpDelete("DeleteFile/{id:int}")]
        public async Task<ActionResult> DeleteFile(int id)
        {
            try
            {
                // obtenemos el archivo multimedia
                MultimediaDTO multimediaDTO = (await RepositoryMultimedia.GetAsync(n => n.IdPublicacion == id)).FirstOrDefault();

                // los eliminamos
                if (multimediaDTO == null)
                {
                    return NotFound("El archivo multimedia no se ha encontrado");
                }

                Multimedia multimedia = RepositoryMultimedia.GetMapper().Map<Multimedia>(multimediaDTO);
                string directorio = string.Format(RUTA_ARCHIVOS_MULTIMEDIA, multimedia.IdPublicacion);
                await SaveFile.Delete(directorio, multimediaDTO.Ruta);
                RepositoryMultimedia.Delete(multimedia);
                await RepositoryMultimedia.Commit();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Guarda los archivos adjuntos al crear una publicacion
        /// </summary>
        /// <param name="nodo">publicación creada</param>
        /// <param name="multimedias">lista de archivos multimedia adjunto a la publicacion</param>
        /// <returns>No retorna nada relevante</returns>
        private async Task GuardarMultimedias(Publicacion nodo, List<IFormFile> multimedias)
        {
            // se valida si adjunto multimedia (imagenes, videos, etc.)
            if (multimedias == null || multimedias.Count <= 0)
            {
                return;
            }

            try
            {
                // si es asi las guardamos
                foreach (IFormFile archivo in multimedias)
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

                        // verificamos que no exista igual
                        Multimedia multimedia = null;
                        MultimediaDTO multimediaDTO = (await RepositoryMultimedia.GetAsync(n => n.Nombre == nombre && n.Extension == extension)).FirstOrDefault();

                        if (multimediaDTO != null)
                        {
                            multimedia = RepositoryMultimedia.GetMapper().Map<Multimedia>(multimediaDTO);
                            multimedia.Ruta = ruta;
                            multimedia.Nombre = nombre;
                            multimedia.Extension = extension;
                            multimedia.FechaAlta = DateTime.Now;
                            multimedia.IdPublicacion = nodo.Id;

                            RepositoryMultimedia.Update(multimedia);
                        }
                        else
                        {
                            multimedia = new Multimedia()
                            {
                                Extension = extension,
                                FechaAlta = DateTime.Now,
                                IdPublicacion = nodo.Id,
                                Nombre = nombre,
                                Ruta = ruta
                            };

                            await RepositoryMultimedia.CreateAsync(multimedia);
                        }

                        await RepositoryMultimedia.Commit();

                        memory.Close();
                        await memory.DisposeAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Guarda un archivo adjunto a una publicacion
        /// </summary>
        /// <param name="nodo">publicación</param>
        /// <param name="archivo">archivo multimedia</param>
        /// <returns>No retorna nada relevante</returns>
        private async Task GuardarMultimedia(Publicacion nodo, IFormFile archivo)
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

                    // verificamos que no exista igual
                    Multimedia multimedia = null;
                    MultimediaDTO multimediaDTO = (await RepositoryMultimedia.GetAsync(n => n.Nombre == nombre && n.Extension == extension)).FirstOrDefault();

                    if (multimediaDTO != null)
                    {
                        multimedia = RepositoryMultimedia.GetMapper().Map<Multimedia>(multimediaDTO);
                        multimedia.Ruta = ruta;
                        multimedia.Nombre = nombre;
                        multimedia.Extension = extension;
                        multimedia.FechaAlta = DateTime.Now;
                        multimedia.IdPublicacion = nodo.Id;

                        RepositoryMultimedia.Update(multimedia);
                    }
                    else
                    {
                        multimedia = new Multimedia()
                        {
                            Extension = extension,
                            FechaAlta = DateTime.Now,
                            IdPublicacion = nodo.Id,
                            Nombre = nombre,
                            Ruta = ruta
                        };

                        await RepositoryMultimedia.CreateAsync(multimedia);
                    }

                    await RepositoryMultimedia.Commit();

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
