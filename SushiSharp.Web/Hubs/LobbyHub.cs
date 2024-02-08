using Microsoft.AspNetCore.SignalR;

using Newtonsoft.Json;

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

    private Player GetContextPlayer(string user)
    {
        return new Player { Id = Context.ConnectionId, Name = user };
    }

    public async Task CreateGame(string user, IGameService gameService)
    {
        var game = await gameService.CreateNewGame(GetContextPlayer(user));
        var games = await gameService.GetGames();

        await Clients.All.SendAsync("GameList",JsonConvert.SerializeObject(games));

        await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);
        
        await Clients.Caller.SendAsync("SetGame",JsonConvert.SerializeObject(game));
    }

    public async Task JoinGame(string user, string gameId, IGameService gameService)
    {
        var game = await gameService.JoinGame(gameId, GetContextPlayer(user));
        var games = await gameService.GetGames();
        
        await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);
        
        await Clients.Group(game.Id).SendAsync("SetGame",JsonConvert.SerializeObject(game));
        await Clients.All.SendAsync("GameList",JsonConvert.SerializeObject(games));
    }

    public async Task Init(IChatService chatService, IGameService gameService)
    {
        var messages = await chatService.GetMessages("Lobby");
        var games = await gameService.GetGames();
        
        await Clients.Caller.SendAsync("LobbyChat", messages);
        await Clients.Caller.SendAsync("GameList", JsonConvert.SerializeObject(games));
    }
}