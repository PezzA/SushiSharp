using Akka.Actor;
using Akka.Hosting;

using Microsoft.AspNetCore.SignalR;

using Newtonsoft.Json;

using SushiSharp.Game;
using SushiSharp.Game.Chat;
using SushiSharp.Web.Actors;

namespace SushiSharp.Web.Hubs;

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

        await chatService.Add("Lobby", new ChatMessage(player, DateTime.Now, message));

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

    public Task LeaveGame(string gameId) => GenericPlayerGameRequest<GameActorMessages.LeaveGameRequest>(gameId);
    
    public Task StartGame(string gameId) => GenericPlayerGameRequest<GameActorMessages.StartGameRequest>(gameId);
    
    public Task JoinGame(string gameId) => GenericPlayerGameRequest<GameActorMessages.JoinGameRequest>(gameId);

    public async Task InitClient(string userName, IChatService chatService)
    {
        var player = await playerService.AddPlayer(userName, Context.ConnectionId);

        await BroadcastLobbyChat(chatService, true);

        await GenericPlayerRequest<GameActorMessages.GetGameListRequest>();
    }
}