using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xchangez.Models;

namespace Xchangez
{
    public class Fun
    {
        public static async Task<Usuario> GetAuthUser(XchangezContext xchangezContext, ClaimsPrincipal user)
        {
            string strId = Convert.ToString(user.FindFirst(ClaimTypes.Name)?.Value);
            int id = !string.IsNullOrEmpty(strId) ? Convert.ToInt32(strId) : -1;

            return await xchangezContext.Usuarios.FirstOrDefaultAsync(n => n.Id == id);
        }

        public static async Task<float> GetAverage(XchangezContext xchangezContext, int idUsuario)
        {
            var valoraciones = await xchangezContext.Valoraciones.Where(n => n.IdUsuarioValorado == idUsuario).ToListAsync();
            double promedio = valoraciones.Count != 0 ? valoraciones.Select(n => n.Cantidad).Average() : 0;

            return (float)promedio;
        }

        public static async Task<int> GetCantidadSeguidores(XchangezContext xchangezContext, int idUsuario)
        {
            return await xchangezContext.Seguidores.Where(n => n.IdUsuarioSeguido == idUsuario).CountAsync();
        }

        public static async Task<int> GetCantidadSeguidos(XchangezContext xchangezContext, int idUsuario)
        {
            return await xchangezContext.Seguidores.Where(n => n.IdUsuarioSeguidor == idUsuario).CountAsync();
        }
    }
}
