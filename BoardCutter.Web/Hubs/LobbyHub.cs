using Akka.Actor;
using Akka.Hosting;

using BoardCutter.Core.Web.Shared.Chat;
using BoardCutter.Games.SushiGo;
using BoardCutter.Games.SushiGo.Actors.Game;
using BoardCutter.Games.SushiGo.Actors.GameManager;
using BoardCutter.Games.SushiGo.Players;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using Newtonsoft.Json;

namespace BoardCutter.Web.Hubs;

[Authorize]
public class LobbyHub(IRequiredActor<GameManagerActor> gameManagerActor, IPlayerService playerService) : Hub
{
    private readonly IActorRef _gameManagerActor = gameManagerActor.ActorRef;

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

        await chatService.Add("Lobby", new ChatMessage(player.Name, player.AvatarPath(), DateTime.Now, message));

        await BroadcastLobbyChat(chatService);
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
    
    public Task CreateGame() => GenericPlayerRequest<GameActorMessages.CreateGameRequest>();

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

    public async Task InitClient(IChatService chatService)
    {
        var loggedInUserName = Context.User?.Identity?.Name;

        if (string.IsNullOrEmpty(loggedInUserName))
        {
            throw new InvalidOperationException("Could no determine logged in user");
        }

        var player = await playerService.AddPlayer(loggedInUserName, Context.ConnectionId);

        await Clients.Caller.SendAsync("SetIdentity", player.Id);

        await BroadcastLobbyChat(chatService, true);

        await GenericPlayerRequest<GameActorMessages.GetGameListRequest>();
    }
}