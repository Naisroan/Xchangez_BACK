using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xchangez.DTOs;
using Xchangez.Interfaces;
using Xchangez.Models;

namespace Xchangez.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IRepository<Mensaje, MensajeDTO> Repository;

        public ChatHub(IRepository<Mensaje, MensajeDTO> repository)
        {
            Repository = repository;
        }

        public async Task SendMessage(MensajeDTO mensaje)
        {
            Usuario authUser = await Fun.GetAuthUser(Repository.GetContext(), Context.User);

            var remitente = Clients.User("");

            if (authUser == null)
            {
                return;
            }

            await Clients.All.SendAsync("receiveMessage", mensaje);
        }

        public async Task SendAnonymusMessage(int id, int idDestinatario, string mensaje)
        {
            Usuario authUser = await Fun.GetAuthUser(Repository.GetContext(), Context.User);

            var remitente = Clients.User("");

            if (authUser == null)
            {
                return;
            }

            var identity = (ClaimsIdentity)Context.User.Identity;
            var tmp = identity.FindFirst(ClaimTypes.NameIdentifier);

            await Clients.All.SendAsync("receiveMessage", id, mensaje);
        }

        public async Task SendTestMessage(string mensaje)
        {
            await Clients.All.SendAsync("receiveMessage", mensaje);
        }
    }
}
