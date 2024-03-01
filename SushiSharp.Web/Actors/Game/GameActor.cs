using Akka.Actor;

using Newtonsoft.Json;

using SushiSharp.Cards;
using SushiSharp.Cards.Decks;
using SushiSharp.Cards.Shufflers;
using SushiSharp.Game;
using SushiSharp.Web.Actors.HubWriter;

namespace SushiSharp.Web.Actors.Game;

/// <summary>
/// GameActor holds all the state for an individual game
/// </summary>
public class GameActor : ReceiveActor
{
    // Dependencies
    private readonly Player _creator;
    private readonly string _gameId;
    private readonly ICardShuffler _cardShuffler;
    private readonly IActorRef _hubWriterActor;
    private bool _awaitingPlay;

    // Internal State
    private GameState _gameState;

    private Dictionary<string, Card[]> _playList = [];

    private int _round = 1;

    private void ProcessPlays()
    {
        foreach ((string playerId, Card[] play) in _playList)
        {
            var tab = _gameState.PlayerBoardStates.Single(p => p.PlayerId == playerId);

            foreach (var card in play)
            {
                tab.Played.Add(card);
                tab.Hand.Remove(card);
            }

            if (play.Length > 1)
            {
                // TODO should validate it was in the hand to start with?
                var card = tab.Played.First(c => c.Type == CardType.Chopsticks);
                tab.Played.Remove(card);
                tab.Hand.Add(card);
            }
        }
    }

    private void RotateHands()
    {
        
    }

    private bool AllCardsPlayed()
    {
        return _gameState.PlayerBoardStates.First().Hand.Count == 0;
    }

    private void ScoreGame()
    {
    }

    private void UpdateGameState()
    {
        _awaitingPlay = false;
        // take all the plays and process hands
        ProcessPlays();

        if (!AllCardsPlayed())
        {
            RotateHands();
            return;
        }

        if (_round == 3)
        {
            ScoreGame();
            return;
        }

        DealCards();
        _round += 1;

        _awaitingPlay = true;
    }

    public GameActor(ICardShuffler cardShuffler, IActorRef hubWriterActor, Player creator, string gameId)
    {
        _cardShuffler = cardShuffler;
        _hubWriterActor = hubWriterActor;
        _creator = creator;
        _gameId = gameId;

        _gameState = new GameState(creator, gameId);
        Receive<GameActorMessages.StartGameRequest>(StartGame);
        Receive<GameActorMessages.JoinGameRequest>(JoinGame);
        Receive<GameActorMessages.GamePlayRequest>(GamePlay);
    }

    private void GamePlay(GameActorMessages.GamePlayRequest message)
    {
        if (!_awaitingPlay)
        {
            SendError(message.Player.ConnectionId, "Game is not expecting a play message.");
            return;
        }

        _playList[message.Player.Id] = message.Played;

        if (_playList.Count == _gameState.GameData.Players.Count)
        {
            _awaitingPlay = false;
            UpdateGameState();
            _playList = [];
            _awaitingPlay = true;
        }

        SendPlayerStatus();
    }

    protected override void PreStart()
    {
        _gameState = new GameState(_creator, _gameId);

        _hubWriterActor.Tell(new HubWriterMessages.AddToGroup(_gameId, _creator.ConnectionId));

        Context.Parent.Tell(new GameActorMessages.UpdateGameNotification(_gameState.GameData));

        _hubWriterActor.Tell(new HubWriterMessages.WriteClient(
            _creator.ConnectionId,
            ServerMessages.SetGame,
            JsonConvert.SerializeObject(_gameState.GameData)));
    }

    private void SendError(string connectionId, string message)
    {
        _hubWriterActor.Tell(new HubWriterMessages.WriteClient(connectionId,
            ServerMessages.ErrorMessage, message));
    }

    private void JoinGame(GameActorMessages.JoinGameRequest message)
    {
        if (_gameState.GameData.Players.Count >= _gameState.GameData.Parameters.MaxPlayers)
        {
            SendError(message.Player.ConnectionId, "Max players in game.");
            return;
        }

        if (_gameState.GameData.Players.Any(p => p.Id == message.Player.Id))
        {
            SendError(message.Player.ConnectionId, "Client is already in a game.");
            return;
        }

        _gameState.GameData.Players.Add(message.Player);

        _hubWriterActor.Tell(new HubWriterMessages.AddToGroup(message.GameId, message.Player.ConnectionId));

        _hubWriterActor.Tell(new HubWriterMessages.WriteGroup(
            _gameState.GameData.Id,
            ServerMessages.SetGame,
            JsonConvert.SerializeObject(_gameState.GameData)));

        Context.Parent.Tell(new GameActorMessages.UpdateGameNotification(_gameState.GameData));
    }

    private void DealCards()
    {
        if (_gameState?.GameDeck == null) throw new InvalidOperationException("No game state or game Deck");

        for (int i = 0; i < 9; i++)
        {
            foreach (var boardState in _gameState.PlayerBoardStates)
            {
                (List<Card> cards, _) = _gameState.GameDeck.Draw(1);

                // not going to check for end of deck
                boardState.Hand.Add(cards[0]);
            }
        }
    }

    private void ShuffleDeck()
    {
        _gameState.GameDeck = new Deck(_cardShuffler, SushiGoClassic.GetDeck());
        _gameState.GameDeck.Shuffle();
    }

    private void SendPlayerStatus()
    {
        var statii = _gameState.GameData.Players.ToDictionary(p => p.Id, p => _playList.ContainsKey(p.Id));
        _hubWriterActor.Tell(new HubWriterMessages.WriteGroup(_gameId, ServerMessages.SetPlayStatus,
            JsonConvert.SerializeObject(statii)));
    }

    private void SendFullBoardState()
    {
        _hubWriterActor.Tell(new HubWriterMessages.WriteGroup(_gameState.GameData.Id, ServerMessages.SetGame,
            JsonConvert.SerializeObject(_gameState.GameData)));

        foreach (var player in _gameState.GameData.Players)
        {
            _hubWriterActor.Tell(new HubWriterMessages.WriteClient(player.ConnectionId, ServerMessages.SetPlayerData,
                JsonConvert.SerializeObject(_gameState.GetPublicDataForPlayer(player.Id))));
        }

        SendPlayerStatus();
    }

    private void StartGame(GameActorMessages.StartGameRequest message)
    {
        // TODO - Only the person who created the game should be able to start it.

        _gameState.GameData.Status = GameStatus.Running;
        Context.Parent.Tell(new GameActorMessages.UpdateGameNotification(_gameState.GameData));

        _gameState.PlayerBoardStates = _gameState.GameData.Players
            .Select(p => new Tableau(p.Id, [], [], []))
            .ToList();

        ShuffleDeck();

        DealCards();

        SendFullBoardState();

        _awaitingPlay = true;
    }
}