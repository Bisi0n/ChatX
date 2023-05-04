using Microsoft.AspNetCore.SignalR;


namespace ChatX.Hubs
{
    public class Chathub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);

            //Save later to db
        }

        public async Task AddEmoji(string user, string message, string emoji)
        {
            await Clients.All.SendAsync($"ReceiveReaction", user, message, emoji);
        }
    }
}
