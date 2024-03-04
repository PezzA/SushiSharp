using Akka.Actor;

using Newtonsoft.Json;

using SushiSharp.Cards.Shufflers;
using SushiSharp.Game.Actors.ClientWriter;
using SushiSharp.Game.Actors.Game;
using SushiSharp.Game.Models;

namespace SushiSharp.Game.Actors.GameManager;

public class GameManagerActor : ReceiveActor
{
    private readonly ICardShuffler _cardShuffler;
    private readonly IActorRef _clientWriterActor;

    private readonly Dictionary<string, IActorRef> _gameActorList = [];
    private readonly Dictionary<string, PublicVisible> _gameDataList = [];

    public GameManagerActor(ICardShuffler cardShuffler, IActorRef clientWriteActor)
    {
        _cardShuffler = cardShuffler;
        _clientWriterActor = clientWriteActor;
        Receive<GameActorMessages.CreateGameRequest>(CreateGame);
        Receive<GameActorMessages.JoinGameRequest>(JoinGame);
        Receive<GameActorMessages.LeaveGameRequest>(LeaveGame);
        Receive<GameActorMessages.StartGameRequest>(StartGame);
        Receive<GameActorMessages.UpdateGameNotification>(UpdateGame);
        Receive<GameActorMessages.GetGameListRequest>(GetGameList);
        Receive<GameActorMessages.GamePlayRequest>(SubmitTurn);
    }

    private void GetGameList(GameActorMessages.GetGameListRequest message)
    {
        var gameList = _gameDataList.Select(entry => entry.Value).ToArray();

        _clientWriterActor.Tell(new ClientWriterActorMessages.WriteClient(message.Player.ConnectionId,
            ServerMessages.GameList,
            JsonConvert.SerializeObject(gameList)));
    }

    private void UpdateGame(GameActorMessages.UpdateGameNotification message)
    {
        _gameDataList[message.GameData.Id] = message.GameData;

        var gameList = _gameDataList.Select(entry => entry.Value).ToArray();

        // Debug to enable test.
        Sender?.Tell(message);

        _clientWriterActor.Tell(new ClientWriterActorMessages.WriteAll(ServerMessages.GameList,
            JsonConvert.SerializeObject(gameList)));
    }

    private void CreateGame(GameActorMessages.CreateGameRequest message)
    {
        var gameId = Guid.NewGuid().ToString();

        var gameActor = Context.ActorOf(Props.Create(
            () => new GameActor(_cardShuffler, _clientWriterActor, message.Player, gameId)));

        gameActor.Tell(message);
        
        _gameActorList[gameId] = gameActor;
    }

    private void GenericPlayerGameRequest<T>(T message) where T : GameActorMessages.PlayerGameRequest
    {
        if (!_gameActorList.ContainsKey(message.GameId))
        {
            _clientWriterActor.Tell(new ClientWriterActorMessages.WriteClient(message.Player.ConnectionId,
                ServerMessages.ErrorMessage, $"{typeof(T)}: Game could not be found.  GameId :{message.GameId}"));

            return;
        }

        _gameActorList[message.GameId].Tell(message);
    }

    private void StartGame(GameActorMessages.StartGameRequest message) => GenericPlayerGameRequest(message);

    private void JoinGame(GameActorMessages.JoinGameRequest message) => GenericPlayerGameRequest(message);

    private void LeaveGame(GameActorMessages.LeaveGameRequest message) => GenericPlayerGameRequest(message);

    private void SubmitTurn(GameActorMessages.GamePlayRequest message)
    {
        if (!_gameActorList.ContainsKey(message.GameId))
        {
            _clientWriterActor.Tell(new ClientWriterActorMessages.WriteClient(message.Player.ConnectionId,
                ServerMessages.ErrorMessage, $"GamePlayRequest: Game could not be found.  GameId :{message.GameId}"));

            return;
        }

        _gameActorList[message.GameId].Tell(message);
    }
}