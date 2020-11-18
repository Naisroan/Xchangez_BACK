using Microsoft.EntityFrameworkCore;
using System;
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
    }
}
