using Microsoft.AspNetCore.SignalR;

using Newtonsoft.Json;

using SushiSharp.Game;
using SushiSharp.Game.Chat;

namespace SushiSharp.Web.Hubs;

public class LobbyHub : Hub
{
    private async Task BroadcastGameList(IGameService gameService, bool callerOnly = false)
    {
        var games = await gameService.GetGames();

        if (callerOnly)
        {
            await Clients.Caller.SendAsync("GameList", JsonConvert.SerializeObject(games));
        }
        else
        {
            await Clients.All.SendAsync("GameList", JsonConvert.SerializeObject(games));
        }
    }

    private async Task BroadcastLobbyChat(IChatService chatService, bool callerOnly = false)
    {
        var messages = await chatService.GetMessages("Lobby");

        if (callerOnly)
        {
            await Clients.Caller.SendAsync("LobbyChat", JsonConvert.SerializeObject(messages));
        }
        else
        {
            await Clients.All.SendAsync("LobbyChat", JsonConvert.SerializeObject(messages));
        }
    }

    public async Task SendLobbyChat(string message, IChatService chatService, IPlayerService playerService)
    {
        var player = await playerService.GetPlayerByConnectionId(Context.ConnectionId);

        if (player == null)
        {
            return;
        }

        await chatService.Add("Lobby", new ChatMessage(player, DateTime.Now, message));

        await BroadcastLobbyChat(chatService);
    }


    public async Task CreateGame(IGameService gameService, IPlayerService playerService)
    {
        var player = await playerService.GetPlayerByConnectionId(Context.ConnectionId);

        // TODO : Could not find player
        if (player == null)
        {
            return;
        }

        var game = await gameService.CreateNewGame(player);

        await BroadcastGameList(gameService);

        await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);

        await Clients.Caller.SendAsync("SetGame", JsonConvert.SerializeObject(game));
    }

    public async Task LeaveGame(string gameId, IGameService gameService, IChatService chatService,
        IPlayerService playerService)
    {
        var player = await playerService.GetPlayerByConnectionId(Context.ConnectionId);

        // TODO : Could not find player
        if (player == null)
        {
            return;
        }

        await gameService.LeaveCurrentGame(player, gameId);

        await Groups.RemoveFromGroupAsync(@Context.ConnectionId, gameId);

        var game = await gameService.GetGame(gameId);

        await Clients.Group(gameId).SendAsync("SetGame", JsonConvert.SerializeObject(game));

        await BroadcastGameList(gameService);
        await BroadcastLobbyChat(chatService);

        await Clients.Caller.SendAsync("SetGame", null);
    }

    public async Task StartGame(string gameId, IGameService gameService)
    {
        var game = await gameService.StartGame(gameId);

        await Clients.Group(gameId).SendAsync("SetGame", JsonConvert.SerializeObject(game));

        await BroadcastGameList(gameService);
    }

    public async Task JoinGame(string gameId, IGameService gameService, IPlayerService playerService)
    {
        var player = await playerService.GetPlayerByConnectionId(Context.ConnectionId);

        // player not found
        if (player == null)
        {
            return;
        }

        var game = await gameService.JoinGame(gameId, player);

        if (game == null)
        {
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);

        await Clients.Group(game.Id).SendAsync("SetGame", JsonConvert.SerializeObject(game));

        await BroadcastGameList(gameService);
    }

    public async Task Init(string userName, IChatService chatService, IGameService gameService,
        IPlayerService playerService)
    {
        var player = await playerService.AddPlayer(userName, Context.ConnectionId);

        var currentGame = await gameService.GetGameByPlayer(player);

        if (currentGame != null)
        {
            await Clients.Caller.SendAsync("SetGame", JsonConvert.SerializeObject(currentGame));
            return;
        }

        await BroadcastLobbyChat(chatService, true);
        await BroadcastGameList(gameService, true);
    }
}