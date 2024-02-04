using Microsoft.AspNetCore.SignalR;

using SushiSharp.Cards;
using SushiSharp.Game;
using SushiSharp.Game.Chat;

namespace SushiSharp.Web.Hubs;

public class LobbyHub : Hub
{
    public async Task SendLobbyChat(string user, string message, IChatService chatService)
    {
        await chatService.Add("Lobby", new ChatMessage(user, DateTime.Now, message));

        var messages = await chatService.GetMessages("Lobby");
        await Clients.All.SendAsync("LobbyChat", messages);
    }

    public async Task CreateGame(string user, IGameService gameService)
    {
        GameState game = await gameService.CreateNewGame(new Player { Id = Context.ConnectionId, Name = user });

        List<GameState> games = await gameService.GetGames();

        await Clients.All.SendAsync("GameList", games.ToArray());
    }

    public async Task GetLobbyChat(IChatService chatService)
    {
        var messages = await chatService.GetMessages("Lobby");
        await Clients.Caller.SendAsync("LobbyChat", messages);
    }
}