using Microsoft.AspNetCore.SignalR;

using SushiSharp.Web.Chat;

namespace SushiSharp.Web.Hubs;

public class LobbyHub : Hub
{
    public async Task SendMessage(string user, string message, IChatService chatService)
    {
        await chatService.Add("Lobby", new ChatMessage(user, DateTime.Now, message));

        var messages = await chatService.GetMessages("Lobby");
        await Clients.All.SendAsync("ReceiveMessages", messages);
    }

    public async Task GetMessages(IChatService chatService)
    {
        var messages = await chatService.GetMessages("Lobby");
        await Clients.Caller.SendAsync("ReceiveMessages", messages);
    }
}