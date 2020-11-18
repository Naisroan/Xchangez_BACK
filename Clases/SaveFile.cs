using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using Xchangez.Interfaces;

namespace Xchangez.Clases
{
    public class SaveFile : IFile
    {
        private readonly IWebHostEnvironment Environment;

        private readonly IHttpContextAccessor Context;

        public SaveFile(IWebHostEnvironment env, IHttpContextAccessor context)
        {
            Environment = env;
            Context = context;
        }

        public async Task<string> SaveAsync(byte[] contenido, string nombre, string extension, string contenedor, string contentType)
        {
            string folder = Path.Combine(Environment.WebRootPath, contenedor);
            nombre = $"{nombre}{extension}";

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string ruta = Path.Combine(folder, nombre);

            if (File.Exists(ruta))
            {
                File.Delete(ruta);
            }

            await File.WriteAllBytesAsync(ruta, contenido);

            string rutaActual = $"{Context.HttpContext.Request.Scheme}://{Context.HttpContext.Request.Host}";

            return Path.Combine(rutaActual, contenedor, nombre).Replace("\\", "/");
        }

        public Task Delete(string contenedor, string ruta)
        {
            if (string.IsNullOrEmpty(ruta))
            {
                return Task.FromResult(0);
            }

            string directorio = Path.Combine(Environment.WebRootPath, contenedor, Path.GetFileName(ruta));

            if (!File.Exists(directorio))
            {
                return Task.FromResult(0);
            }

            File.Delete(directorio);

            return Task.FromResult(0);
        }

        public async Task<string> Edit(byte[] contenido, string nombre, string extension, string contenedor, string ruta, string contentType)
        {
            await Delete(contenedor, ruta);
            return await SaveAsync(contenido, nombre, extension, contenedor, contentType);
        }
    }
}
