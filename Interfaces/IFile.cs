using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xchangez.Interfaces
{
    public interface IFile
    {
        Task<string> SaveAsync(byte[] contenido, string nombre, string extension, string contenedor, string contentType);

        Task<string> Edit(byte[] contenido, string nombre, string extension, string contenedor, string ruta, string contentType);

        Task Delete(string contenedor, string ruta);
    }
}
