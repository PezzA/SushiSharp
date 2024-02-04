using SushiSharp.Game.Chat;

namespace SushiSharp.Web.Hubs;

public interface ILobbyHub
{
    public Task SendLobbyChat(string user, string message, IChatService chatService);

    public Task<List<ChatMessage>> GetLobbyChat(IChatService chatService);
    
}