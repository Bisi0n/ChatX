using ChatX_Blazor.Data;
using ChatX_Blazor.Models;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Security.Principal;

namespace ChatX_Blazor.Hubs
{
    public class ChatHub : Hub
    {
        private readonly FakeDb _db;
        List<string> _usersCurrentlyTyping;

        public ChatHub(FakeDb database)
        {
            _db = database;
            _usersCurrentlyTyping = new();
        }

        public async Task SendMessage(string sender, string messageContent)
        {
            
            Message message = new()
            {
                Content = messageContent,
                Sender = sender,
                TimeStamp = DateTime.UtcNow,
            };

            _db.AddMessage(message);

            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        public async Task DeleteMessage(int id)
        {
            Message? message = _db.GetAllMessages().Where(m => m.Id == id).SingleOrDefault();

            if (message is not null)
            {
                _db.RemoveMessage(message);
            }

            await Clients.All.SendAsync("DeleteMessage", id);
        }

        public async Task LoadPreviousMessages()
        {
            List<Message> messages = _db.GetAllMessages();

            await Clients.Caller.SendAsync("ReceiveMessageHistory", messages);
        }

        public async Task UserTyping(string user, bool isTyping)
        {
            if (isTyping)
            {
                if (!_usersCurrentlyTyping.Contains(user))
                {
                    _usersCurrentlyTyping.Add(user);
                }
            }
            else
            {
                _usersCurrentlyTyping.Remove(user);
            }

            await Clients.All.SendAsync("CurrentlyTyping", _usersCurrentlyTyping);
        }
    }
}
