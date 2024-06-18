using Akka.Actor;
using Akka.Hosting;
using BoardCutter.Core.Players;
using BoardCutter.Core.Web.Shared.Chat;
using BoardCutter.Games.SushiGo;
using BoardCutter.Games.SushiGo.Actors.Game;
using BoardCutter.Games.SushiGo.Actors.GameManager;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using Newtonsoft.Json;

namespace BoardCutter.Web.Hubs;

[Authorize]
public class LobbyHub(IRequiredActor<GameManagerActor> gameManagerActor, IPlayerService playerService, IChatService chatService) : Hub
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

    private async Task GenericPlayerRequest<T>() where T : GameActorMessages.PlayerRequest
    {
        var player = await playerService.GetPlayerByConnectionId(Context.ConnectionId);

        if (player == null)
        {
            return;
        }
        var message = (T)Activator.CreateInstance(typeof(T), player)!;
        
        _gameManagerActor.Tell(message);
    }
    
    private async Task GenericPlayerGameRequest<T>(string gameId) where T : GameActorMessages.PlayerGameRequest
    {
        var player = await playerService.GetPlayerByConnectionId(Context.ConnectionId);

        if (player == null)
        {
            return;
        }
        var message = (T)Activator.CreateInstance(typeof(T), player, gameId)!;
        
        _gameManagerActor.Tell(message);
    }

    public async Task CreateGame(string gameTag)
    {
        var player = await playerService.GetPlayerByConnectionId(Context.ConnectionId);

        if (player == null)
        {
            return;
        }

        var message = new GameActorMessages.CreateGameRequest(player, gameTag);
        
        _gameManagerActor.Tell(message);
    }

    public async Task SubmitTurn(string gameId, string payload)
    {
        var cards = JsonConvert.DeserializeObject<List<Card>>(payload);
       
        var player = await playerService.GetPlayerByConnectionId(Context.ConnectionId);

        if (player == null)
        {
            return;
        }
        
        
        _gameManagerActor.Tell(new GameActorMessages.GamePlayRequest(player, gameId, cards));
    }
    
    public Task LeaveGame(string gameId) => GenericPlayerGameRequest<GameActorMessages.LeaveGameRequest>(gameId);
   
    public Task StartGame(string gameId) => GenericPlayerGameRequest<GameActorMessages.StartGameRequest>(gameId);
    
    public Task JoinGame(string gameId) => GenericPlayerGameRequest<GameActorMessages.JoinGameRequest>(gameId);
    
    // Someone landing on an instance of a game, should be created at this point
    public async Task InitClientGame(string gameId)
    {
        var loggedInUserName = Context.User?.Identity?.Name;

        if (string.IsNullOrEmpty(loggedInUserName)) throw new InvalidDataException("Should have a user");

        var player = await playerService.AddOrUpdatePlayer(loggedInUserName, Context.ConnectionId, true);

        if (player == null) throw new InvalidDataException("Could not find user");
        
        await Clients.Caller.SendAsync("SetIdentity", player.Id);
        
        _gameManagerActor.Tell(new GameActorMessages.ConnectGameRequest(player, gameId));
    }
    
    public async Task InitClient()
    {
        var loggedInUserName = Context.User?.Identity?.Name;

        if (string.IsNullOrEmpty(loggedInUserName))
        {
            throw new InvalidOperationException("Could no determine logged in user");
        }

        var player = await playerService.AddOrUpdatePlayer(loggedInUserName, Context.ConnectionId, false);

        await Clients.Caller.SendAsync("SetIdentity", player.Id);

        await BroadcastLobbyChat(true);

        await GenericPlayerRequest<GameActorMessages.GetGameListRequest>();
    }
}