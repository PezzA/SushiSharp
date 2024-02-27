using Akka.Actor;

using Microsoft.AspNetCore.SignalR;

using SushiSharp.Cards.Shufflers;
using SushiSharp.Game.ViewModels;
using SushiSharp.Web.Hubs;

namespace SushiSharp.Web.Actors;

public class GameManagerActor : ReceiveActor
{
    private readonly IHubContext<LobbyHub> _hubContext;
    private readonly ICardShuffler _cardShuffler;
    
    private readonly Dictionary<string, IActorRef> _gameActorList = [];
    private readonly Dictionary<string, PublicGameData> _gameDataList = [];

    public GameManagerActor(IHubContext<LobbyHub> hubContext, ICardShuffler cardShuffler)
    {
        _hubContext = hubContext;
        _cardShuffler = cardShuffler;
        Receive<GameActorMessages.CreateGameRequest>(CreateGame);
        ReceiveAsync<GameActorMessages.JoinGameRequest>(JoinGame);
        ReceiveAsync<GameActorMessages.LeaveGameRequest>(LeaveGame);
        ReceiveAsync<GameActorMessages.StartGameRequest>(StartGame);
        ReceiveAsync<GameActorMessages.UpdateGameNotification>(UpdateGame);
        ReceiveAsync<GameActorMessages.GetGameListRequest>(GetGameList);
    }

    private async Task GetGameList(GameActorMessages.GetGameListRequest message)
    {
        await _hubContext.Clients.Client(message.Player.ConnectionId).SendAsync("GameList", _gameDataList.ToArray());
    }

    private async Task UpdateGame(GameActorMessages.UpdateGameNotification message)
    {
        _gameDataList[message.GameData.Id] = message.GameData;
        await _hubContext.Clients.All.SendAsync("GameList", _gameDataList.ToArray());
    }

    private void CreateGame(GameActorMessages.CreateGameRequest message)
    {
        var gameId = Guid.NewGuid().ToString();

        var gameActor = Context.ActorOf(Props.Create(
            () => new GameActor(_hubContext, _cardShuffler, message.Player, gameId)));

        _gameActorList[gameId] = gameActor;
    }

    private async Task GenericPlayerGameRequest<T>(T message) where T : GameActorMessages.PlayerGameRequest
    {
        if (!_gameActorList.ContainsKey(message.GameId))
        {
            await _hubContext.Clients.Client(message.Player.ConnectionId)
                .SendAsync("DebugMessage", $"{typeof(T)}: Game could not be found.  GameId :{message.GameId}");
            return;
        }
        
        _gameActorList[message.GameId].Tell(message);
    }

    private Task StartGame(GameActorMessages.StartGameRequest message) => GenericPlayerGameRequest(message);
    
    private Task JoinGame(GameActorMessages.JoinGameRequest message) => GenericPlayerGameRequest(message);
    
    private Task LeaveGame(GameActorMessages.LeaveGameRequest message) => GenericPlayerGameRequest(message);
}