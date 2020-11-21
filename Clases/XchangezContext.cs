// using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xchangez.Models;

namespace Xchangez
{
    /// <summary>
    /// Clase donde se definen todas las tablas de la base de datos.
    /// <para>
    /// Se definen como una clase abstracta (en la carpeta <see cref="Models"/>), posteriormente se definen en esta clase como objetos de tipo <see cref="DbSet{TEntity}"/>, <see cref="DbSet{TEntity}"/> funcionará para acceder a diferentes métodos del CRUD a la base de datos.
    /// </para>
    /// <example>
    /// Ejemplo:
    /// <code>
    /// public DbSet[Publicacion] Publicaciones { get; set; }
    /// </code>
    /// [Publicacion] es la clase abstracta, y [Publicaciones] es el objeto tipo <see cref="DbSet{TEntity}"/>, este ejemplo se encuentra acá: <see cref="Xchangez.XchangezContext.Publicaciones"/>
    /// </example>
    /// <para>
    /// Para crear o modificar una tabla de la base de datos abra la Consola de Administrador de Paquetes y siga estos pasos:
    /// </para>
    /// <list type="number">
    /// <item>
    /// <description>Es necesario crear o modificar la clase en la carpeta Models (ejemplo: <see cref="Models.PublicacionDTO"/>)</description>
    /// </item>
    /// <item>
    /// <description>Agregarlo a esta clase como DbSet ejemplo: <see cref="Xchangez.XchangezContext.Publicaciones"/></description>
    /// </item>
    /// <item>
    /// <description>Iniciar una migración con el comando: <c>Add-Migration nombre_migracion</c> (el nombre_migracion puede ser el que quieras)</description>
    /// </item>
    /// <item>
    /// <description>Ya hecha la migración se realiza un update con el comando: <c>Update-Database</c></description>
    /// </item>
    /// <item>
    /// <description>Listo, la tabla se agrego a la base de datos.</description>
    /// </item>
    /// </list>
    /// <para>
    /// Si accedemos al objeto tipo <see cref="DbSet{TEntity}"/> llamado <c>Publicaciones</c> (<see cref="Xchangez.XchangezContext.Publicaciones"/>) podemos acceder a métodos para Crear, Leer, Actualizar o Eliminar publicaciones de la base de datos de la tabla <c>Publicacion</c>
    /// </para>
    /// </summary>
    public class XchangezContext : DbContext
    {
        public XchangezContext([NotNullAttribute] DbContextOptions options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Comentario> Comentarios { get; set; }

        public DbSet<Grupo> Grupos { get; set; }

        public DbSet<Lista> Listas { get; set; }

        public DbSet<Mensaje> Mensajes { get; set; }

        public DbSet<Multimedia> Multimedias { get; set; }

        public DbSet<ObjetoLista> ObjetosListas { get; set; }

        public DbSet<Publicacion> Publicaciones { get; set; }

        public DbSet<Valoracion> Valoraciones { get; set; }
    }
}
