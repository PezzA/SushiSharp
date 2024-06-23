using Akka.Actor;
using Akka.Hosting;

using BoardCutter.Core.Actors;
using BoardCutter.Core.Players;
using BoardCutter.Core.Web.Shared.Chat;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using Newtonsoft.Json;

namespace BoardCutter.Web.Hubs;

[Authorize]
public class LobbyHub(
    IRequiredActor<GameManager> gameManagerActor,
    IPlayerService playerService,
    IChatService chatService) : Hub
{
    private readonly IActorRef _gameManagerActor = gameManagerActor.ActorRef;

    private async Task BroadcastLobbyChat(bool callerOnly = false)
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

    public async Task SendLobbyChat(string message)
    {
        var player = await playerService.GetPlayerByConnectionId(Context.ConnectionId);

        if (player == null)
        {
            return;
        }

        await chatService.Add("Lobby", new ChatMessage(player.Name, player.AvatarPath(), DateTime.Now, message));

        await BroadcastLobbyChat();
    }

    public async Task CreateGame(string gameTag)
    {
        var player = await playerService.GetPlayerByConnectionId(Context.ConnectionId);

        if (player == null)
        {
            return;
        }

        _gameManagerActor.Tell(new GameManagerMessages.CreateGameRequest(player, gameTag));
    }

    public async Task JoinGame(string gameId)
    {
        var player = await playerService.GetPlayerByConnectionId(Context.ConnectionId);

        if (player == null)
        {
            return;
        }

        _gameManagerActor.Tell(new GameManagerMessages.JoinGameRequest(player, gameId));
    }

    public async Task InitClient()
    {
        var loggedInUserName = Context.User?.Identity?.Name;

        if (string.IsNullOrEmpty(loggedInUserName))
        {
            throw new InvalidOperationException("Could not determine logged in user");
        }

        var player = await playerService.AddOrUpdatePlayer(loggedInUserName, Context.ConnectionId, false);

        await Clients.Caller.SendAsync("SetIdentity", player.Id);

        var games = await _gameManagerActor.Ask(new GameManagerMessages.GetGameList()) as GameManagerNotifications.BaseGameNotification[];
        await Clients.Caller.SendAsync("GameList", JsonConvert.SerializeObject(games));

        await BroadcastLobbyChat(true);
    }
}