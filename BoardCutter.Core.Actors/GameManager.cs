using Akka.Actor;

namespace BoardCutter.Core.Actors;

public class GameManager : ReceiveActor
{
    private readonly Dictionary<string, Props> _gameActors;

    private readonly Dictionary<string, IActorRef> _gameActorList = [];
    private readonly Dictionary<string, GameManagerNotifications.BaseGameNotification> _gameDataList = [];

    public GameManager(Dictionary<string, Props> gameActors)
    {
        _gameActors = gameActors;

        // Game Notifications
        Receive<GameManagerNotifications.GameCreated>(GameCreated);
        Receive<GameManagerNotifications.GameUpdated>(UpdateGame);
        ReceiveAsync<GameManagerNotifications.GameEnded>(EndGame);

        // Generic Game Messages
        Receive<GameManagerMessages.CreateGameRequest>(CreateGameRequest);
        Receive<GameManagerMessages.JoinGameRequest>(JoinGame);
        Receive<GameManagerMessages.GetGameRequest>(GetGame);

        // Lobby Requests
        Receive<GameManagerMessages.GetGameList>(GetGameList);
    }

    private void GetGame(GameManagerMessages.GetGameRequest message)
    {
        if (!_gameActorList.TryGetValue(message.GameId, out IActorRef? value))
        {
            return;
        }

        Sender.Tell(new GameManagerMessages.GetGameDetails(value));
    }

    private void GameCreated(GameManagerNotifications.GameCreated message)
    {
        _gameDataList[message.Details.Id] = message.Details;
    }

    private async Task EndGame(GameManagerNotifications.GameEnded message)
    {
        var id = message.Details.Id;
        if (!_gameDataList.ContainsKey(id))
        {
            return;
        }

        await _gameActorList[id].GracefulStop(TimeSpan.FromSeconds(10));
        _gameActorList.Remove(id);
        _gameDataList.Remove(id);
    }

    private void GetGameList(GameManagerMessages.GetGameList message)
    {
        var gameList = _gameDataList.Select(entry => entry.Value).ToArray();

        Sender.Tell(gameList);
    }

    private void UpdateGame(GameManagerNotifications.GameUpdated message)
    {
        _gameDataList[message.Details.Id] = message.Details;
    }

    private void CreateGameRequest(GameManagerMessages.CreateGameRequest message)
    {
        var gameId = Guid.NewGuid().ToString();

        var gameActor = Context.ActorOf(_gameActors[message.GameTag], $"{message.GameTag}-{gameId}");

        gameActor.Tell(new GameManagerMessages.CreateGameSpecificRequest(message.Player, gameId));

        _gameActorList.Add(gameId, gameActor);
    }

    private void JoinGame(GameManagerMessages.JoinGameRequest message)
    {
        if (!_gameActorList.TryGetValue(message.GameId, out IActorRef? gameActor))
        {
            return;
        }

        gameActor.Tell(message);
    }
}