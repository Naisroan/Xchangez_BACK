using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xchangez.Interfaces;
using Xchangez.Models;

namespace Xchangez.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // esto indica que necesita autorización por token
    public class ListaController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        private readonly IRepository<Lista, ListaDTO> Repository;
        private readonly IRepository<ObjetoLista, ObjetoListaDTO> RepositoryObjeto;
        private readonly IRepository<Publicacion, PublicacionDTO> RepositoryPublicacion;

        public ListaController(IConfiguration configuration, IRepository<Lista, ListaDTO> repository,
            IRepository<ObjetoLista, ObjetoListaDTO> repositoryObjeto, IRepository<Publicacion, PublicacionDTO> repositoryPublicacion)
        {
            Configuration = configuration;
            Repository = repository;
            RepositoryObjeto = repositoryObjeto;
            RepositoryPublicacion = repositoryPublicacion;
        }

        /// <summary>
        /// Crea una lista en la base de datos en base al usuario autenticado por token
        /// </summary>
        /// <param name="nodo">Lista a crear</param>
        /// <returns>La lista creada junto a sus objetos</returns>
        [HttpPost("Create")]
        public async Task<ActionResult> Create([FromBody] ListaDTO nodo)
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
                Lista nuevo = Repository.GetMapper().Map<Lista>(nodo);
                nuevo.IdUsuario = authUser.Id;

                // creamos y guardamos
                await Repository.CreateAsync(nuevo);
                await Repository.Commit();

                // guardamos los objetos si es que tiene
                if (nodo.Objetos != null && nodo.Objetos.Count > 0)
                {
                    foreach (ObjetoListaDTO objeto in nodo.Objetos)
                    {
                        objeto.IdLista = nuevo.Id;
                        await RepositoryObjeto.CreateAsync(RepositoryObjeto.GetMapper().Map<ObjetoLista>(objeto));
                    }

                    await RepositoryObjeto.Commit();
                }

                // retornamos la lista junto a sus objetos y su nuevo id
                return RedirectToRoute("GetLista", new
                {
                    id = nuevo.Id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene una lista específica
        /// </summary>
        /// <param name="id">Id de la lista a obtener</param>
        /// <returns>Lista</returns>
        [HttpGet("{id:int}", Name = "GetLista")]
        [AllowAnonymous]
        public async Task<ActionResult<ListaDTO>> Get(int id)
        {
            try
            {
                ListaDTO nodo = (await Repository.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (nodo == null)
                {
                    return NotFound("Lista no encontrada"); // retorna un codigo de error 404
                }

                nodo.Objetos = await RepositoryObjeto.GetAsync(n => n.IdLista == id);

                return nodo;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene todas las listas de un usuario
        /// </summary>
        /// <returns>Lista de listas de un usuario</returns>
        [HttpGet("GetByIdUsuario/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ListaDTO>>> GetByIdUsuario(int id)
        {
            try
            {
                Usuario usuario = await Fun.GetAuthUser(Repository.GetContext(), User);
                bool incluirPrivadas = usuario != null && usuario.Id == id;

                var lists = await Repository.GetAsync(n => n.IdUsuario == id && (n.EsPublico || incluirPrivadas));

                foreach (var lista in lists)
                {
                    lista.Objetos = await RepositoryObjeto.GetAsync(o => o.IdLista == lista.Id);
                }

                return lists;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Modifica una lista
        /// </summary>
        /// <param name="id">Id de la lista</param>
        /// <param name="nodo">Lista en si que se modificará</param>
        /// <returns>Estado de success 204</returns>
        [HttpPost]
        public async Task<ActionResult> Put(int id, [FromBody] ListaDTO nodo)
        {
            try
            {
                // se consulta a la bd el nodo a editar y valida que existe
                var nodoDB = (await Repository.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (nodoDB == null)
                {
                    return NotFound("No se encontró la lista a modificar"); // retorna un codigo de error 404
                }

                // se obtiene 'edit' que es el nodo DTO mapeado a su clase abstracta
                Lista lista = Repository.GetMapper().Map<Lista>(nodo);

                lista.Id = id;
                lista.IdUsuario = nodoDB.IdUsuario;
                
                // actualizamos y guardamos
                Repository.Update(lista);
                await Repository.Commit();

                // borramos los objetos
                List<ObjetoListaDTO> objetoListaDTOs = await RepositoryObjeto.GetAsync(n => n.IdLista == lista.Id);

                if (objetoListaDTOs != null && objetoListaDTOs.Count > 0)
                {
                    foreach (ObjetoListaDTO objetoListaDTO in objetoListaDTOs)
                    {
                        ObjetoLista objeto = RepositoryObjeto.GetMapper().Map<ObjetoLista>(objetoListaDTO);
                        RepositoryObjeto.Delete(objeto);
                    }

                }

                // guardamos los objetos si es que tiene
                if (nodo.Objetos != null && nodo.Objetos.Count > 0)
                {
                    foreach (ObjetoListaDTO objeto in nodo.Objetos)
                    {
                        objeto.IdLista = lista.Id;
                        await RepositoryObjeto.CreateAsync(RepositoryObjeto.GetMapper().Map<ObjetoLista>(objeto));
                    }

                }

                await RepositoryObjeto.Commit();

                // retornamos 0 contenido pero sastifactorio
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Elimine una lista
        /// </summary>
        /// <param name="id">Id de la lista a eliminar</param>
        /// <returns>Estado de success 204</returns>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                ListaDTO lista = (await Repository.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (lista == null)
                {
                    return NotFound("La lista que desea retirar no se encuentra");
                }

                Lista nodo = Repository.GetMapper().Map<Lista>(lista);

                List<ObjetoListaDTO> objetoListaDTOs = await RepositoryObjeto.GetAsync(n => n.IdLista == id);

                if (objetoListaDTOs != null && objetoListaDTOs.Count > 0)
                {
                    foreach (ObjetoListaDTO objetoListaDTO in objetoListaDTOs)
                    {
                        ObjetoLista objeto = RepositoryObjeto.GetMapper().Map<ObjetoLista>(objetoListaDTO);
                        RepositoryObjeto.Delete(objeto);
                    }

                    await RepositoryObjeto.Commit();
                }

                // borramos y guardamos
                Repository.Delete(nodo);
                await RepositoryObjeto.Commit();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Agregue un objeto a la lista
        /// </summary>
        /// <param name="id">Id de la lista</param>
        /// <param name="nodo">Objeto de la lista a añadir</param>
        /// <returns>La lista junto a su objeto nuevo</returns>
        [HttpPost("AddObjeto/{id:int}")]
        public async Task<ActionResult> AddObjeto(int id, [FromBody] ObjetoListaDTO nodo)
        {
            try
            {
                // obtenemos el usuario autenticado (mediante token)
                Usuario authUser = await Fun.GetAuthUser(Repository.GetContext(), User);

                if (authUser == null)
                {
                    return NotFound("El usuario autenticado no existe o no se logró obtener su información");
                }

                ListaDTO lista = (await Repository.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (lista == null)
                {
                    return NotFound("La lista de donde desea añadir el objeto no se encuentra");
                }

                // creamos un objeto de la tabla Publicacion desde PublicacionDTO
                ObjetoLista nuevo = RepositoryObjeto.GetMapper().Map<ObjetoLista>(nodo);
                nuevo.IdLista = lista.Id;

                // creamos y guardamos
                await RepositoryObjeto.CreateAsync(nuevo);
                await RepositoryObjeto.Commit();

                // retornamos la publicacion con su nuevo id
                return new CreatedAtRouteResult("GetLista", new
                {
                    id = nuevo.Id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Modifica el objeto de una lista
        /// </summary>
        /// <param name="id">Id del objeto</param>
        /// <param name="nodo">Objeto en si que se modificará</param>
        /// <returns>Estado de success 204</returns>
        [HttpPost("UpdateObjeto/{id:int}")]
        public async Task<ActionResult> Put(int id, [FromForm] ObjetoListaDTO nodo)
        {
            try
            {
                // se consulta a la bd el nodo a editar y valida que existe
                var nodoDB = (await RepositoryObjeto.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (nodoDB == null)
                {
                    return NotFound("No se encontró el objeto a modificar"); // retorna un codigo de error 404
                }

                // se obtiene 'edit' que es el nodo DTO mapeado a su clase abstracta
                ObjetoLista objeto = RepositoryObjeto.GetMapper().Map<ObjetoLista>(nodo);

                objeto.Id = id;

                // actualizamos y guardamos
                RepositoryObjeto.Update(objeto);
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
        /// Elimine un objeto de la lista
        /// </summary>
        /// <param name="id">Id del objeto a eliminar</param>
        /// <returns>La lista sin el objeto removido</returns>
        [HttpDelete("DeleteObjeto/{id:int}")]
        public async Task<ActionResult> DeleteObjeto(int id)
        {
            try
            {
                // obtenemos el usuario autenticado (mediante token)
                Usuario authUser = await Fun.GetAuthUser(Repository.GetContext(), User);

                if (authUser == null)
                {
                    return NotFound("El usuario autenticado no existe o no se logró obtener su información");
                }

                ObjetoListaDTO objeto = (await RepositoryObjeto.GetAsync(n => n.Id == id)).FirstOrDefault();

                if (objeto == null)
                {
                    return NotFound("El objeto que se desea retirar no se encuentra");
                }

                ObjetoLista nodo = Repository.GetMapper().Map<ObjetoLista>(objeto);

                // borramos y guardamos
                RepositoryObjeto.Delete(nodo);
                await RepositoryObjeto.Commit();

                // retornamos la publicacion con su nuevo id
                return new CreatedAtRouteResult("GetLista", new
                {
                    id = objeto.IdLista
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
