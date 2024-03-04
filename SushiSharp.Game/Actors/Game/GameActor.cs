using Akka.Actor;

using Newtonsoft.Json;

using SushiSharp.Cards;
using SushiSharp.Cards.Decks;
using SushiSharp.Cards.Shufflers;
using SushiSharp.Game.Actors.ClientWriter;
using SushiSharp.Game.ViewModels;
using SushiSharp.Web.Actors.Game;

namespace SushiSharp.Game.Actors.Game;

/// <summary>
/// GameActor holds all the state for an individual game
/// </summary>
public class GameActor : ReceiveActor
{
    // Dependencies
    private readonly ICardShuffler _cardShuffler;
    private readonly IActorRef _hubWriterActor;

    // Board State

    /// <summary>
    /// Data that can be viewed in the game list.
    /// </summary>
    private PublicVisible _publicGameData;

    /// <summary>
    /// Player who created the game
    /// </summary>
    private readonly Player _creator;

    /// <summary>
    /// Unique identifier for the game.
    /// </summary>
    private readonly string _gameId;

    /// <summary>
    /// If true, the game is awaiting on input from players
    /// </summary>
    private bool _awaitingPlay;

    /// <summary>
    /// The main draw deck for the game
    /// </summary>
    private Deck? _drawPile;

    /// <summary>
    /// The main discard pile
    /// </summary>
    private List<Card> _discardPile = [];

    /// <summary>
    /// The current round number
    /// </summary>
    private int _round = 1;

    /// <summary>
    /// Current interval view of players hand and board state.
    /// </summary>
    private Dictionary<string, Tableau> _playerBoardStates = [];

    /// <summary>
    /// Contains currently collected turns, typically game state wont progress until all player turns are collected
    /// </summary>
    private Dictionary<string, List<Card>> _playerTurnList = [];

    public GameActor(ICardShuffler cardShuffler, IActorRef hubWriterActor, Player creator, string gameId)
    {
        _cardShuffler = cardShuffler;
        _hubWriterActor = hubWriterActor;
        _creator = creator;
        _gameId = gameId;
        
        _publicGameData = new PublicVisible
        {
            Players = [_creator], Id = _gameId, Status = GameStatus.SettingUp, Parameters = new GameParameters(2)
        };
        
        Receive<GameActorMessages.StartGameRequest>(StartGame);
        Receive<GameActorMessages.CreateGameRequest>(CreateGame);
        Receive<GameActorMessages.JoinGameRequest>(JoinGame);
        Receive<GameActorMessages.GamePlayRequest>(GamePlay);
    }


    private void CreateGame(GameActorMessages.CreateGameRequest message)
    {
        _publicGameData = new PublicVisible
        {
            Players = [_creator], Id = _gameId, Status = GameStatus.SettingUp, Parameters = new GameParameters(2)
        };
        
        Sender.Tell(new GameActorMessages.UpdateGameNotification(_publicGameData));
        
        _hubWriterActor.Tell(new ClientWriterActorMessages.AddToGroup(_gameId, _creator.ConnectionId));

        _hubWriterActor.Tell(new ClientWriterActorMessages.WriteClient(
            _creator.ConnectionId,
            ServerMessages.SetPlayerGame,
            JsonConvert.SerializeObject(_publicGameData)));
    }

    private void ProcessPlays()
    {
        foreach ((string playerId, List<Card> play) in _playerTurnList)
        {
            var tab = _playerBoardStates[playerId];

            foreach (var card in play)
            {
                tab.Played.Add(card);
                tab.Hand.RemoveAt(tab.Hand.FindIndex(c => c.Id == card.Id));
            }

            if (play.Count > 1)
            {
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
        return _playerBoardStates[_creator.Id].Hand.Count == 0;
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
            _publicGameData.Status = GameStatus.Results;

            Sender.Tell(new GameActorMessages.UpdateGameNotification(_publicGameData));
            return;
        }

        DealCards();
        _round += 1;

        _awaitingPlay = true;
    }

    private void GamePlay(GameActorMessages.GamePlayRequest message)
    {
        if (!_awaitingPlay)
        {
            SendError(message.Player.ConnectionId, "Game is not expecting a play message.");
            return;
        }

        if (message.Played == null || !message.Played.Any())
        {
            SendError(message.Player.ConnectionId, "Play was null or contained no cards.");
            return;
        }

        _playerTurnList[message.Player.Id] = message.Played;

        if (_playerTurnList.Count == _publicGameData.Parameters.MaxPlayers)
        {
            _awaitingPlay = false;

            UpdateGameState();

            _playerTurnList = [];

            BroadCastPlayerVisibleData();

            _awaitingPlay = true;
        }

        SendPlayerStatus();
    }


    private void SendError(string connectionId, string message)
    {
        _hubWriterActor.Tell(new ClientWriterActorMessages.WriteClient(
            connectionId,
            ServerMessages.ErrorMessage,
            message));
    }

    private void JoinGame(GameActorMessages.JoinGameRequest message)
    {
        if (_publicGameData.Players.Count >= _publicGameData.Parameters.MaxPlayers)
        {
            SendError(message.Player.ConnectionId, "Max players in game.");
            return;
        }

        if (_publicGameData.Players.Any(p => p.Id == message.Player.Id))
        {
            SendError(message.Player.ConnectionId, "Client is already in a game.");
            return;
        }

        _publicGameData.Players.Add(message.Player);

        _hubWriterActor.Tell(new ClientWriterActorMessages.AddToGroup(
            message.GameId,
            message.Player.ConnectionId));

        _hubWriterActor.Tell(new ClientWriterActorMessages.WriteGroup(
            _publicGameData.Id,
            ServerMessages.SetPlayerGame,
            JsonConvert.SerializeObject(_publicGameData)));

        Sender.Tell(new GameActorMessages.UpdateGameNotification(_publicGameData));
        
    }

    private void DealCards()
    {
        if (_drawPile == null) throw new InvalidOperationException("No game state or game Deck");

        for (int i = 0; i < 9; i++)
        {
            foreach (var boardState in _playerBoardStates)
            {
                (List<Card> cards, _) = _drawPile.Draw(1);

                // not going to check for end of deck
                boardState.Value.Hand.Add(cards[0]);
            }
        }
    }

    private void ShuffleDeck()
    {
        _drawPile = new Deck(_cardShuffler, SushiGoClassic.GetDeck());
        _drawPile.Shuffle();
    }

    private void SendPlayerStatus()
    {
        var statii = _publicGameData.Players.ToDictionary(p => p.Id, p => _playerTurnList.ContainsKey(p.Id));

        _hubWriterActor.Tell(new ClientWriterActorMessages.WriteGroup(
            _gameId,
            ServerMessages.SetPlayerTurnStatus,
            JsonConvert.SerializeObject(statii)));
    }

    private void BroadCastPlayerVisibleData()
    {
        foreach (var player in _publicGameData.Players)
        {
            _hubWriterActor.Tell(new ClientWriterActorMessages.WriteClient(
                player.ConnectionId,
                ServerMessages.SetPlayerVisibleData,
                JsonConvert.SerializeObject(GetPlayerVisibleData(player.Id))));
        }
    }

    private void SendFullBoardState()
    {
        _hubWriterActor.Tell(new ClientWriterActorMessages.WriteGroup(
            _gameId,
            ServerMessages.SetPlayerGame,
            JsonConvert.SerializeObject(_publicGameData)));

        _hubWriterActor.Tell(new ClientWriterActorMessages.WriteGroup(
            _gameId,
            ServerMessages.SetViewerVisibleData,
            JsonConvert.SerializeObject(GetViewerVisibleData())));

        BroadCastPlayerVisibleData();
        SendPlayerStatus();
    }

    private PlayerVisible GetPlayerVisibleData(string playerId) =>
        new()
        {
            PlayerId = playerId,
            Hand = _playerBoardStates[playerId].Hand,
            CurrentPlay = _playerTurnList.TryGetValue(playerId, out List<Card>? value) ? value : []
        };

    private ViewerVisible GetViewerVisibleData() =>
        new()
        {
            RoundNumber = _round,
            CardsInDeck = _drawPile?.CardsRemaining() ?? 0,
            CardsInDiscard = _discardPile.Count,
            OpponentStates = _playerBoardStates.ToDictionary(k => k.Key,
                k => new OpponentState
                {
                    HandSize = k.Value.Hand.Count, Sideboard = k.Value.Side, Played = k.Value.Played
                })
        };

    private void StartGame(GameActorMessages.StartGameRequest message)
    {
        _publicGameData.Status = GameStatus.Running;
        Sender.Tell(new GameActorMessages.UpdateGameNotification(_publicGameData));

        foreach (var player in _publicGameData.Players)
        {
            _playerBoardStates[player.Id] = new Tableau(player.Id, [], [], []);
        }

        _playerTurnList = [];

        ShuffleDeck();

        DealCards();

        SendFullBoardState();

        _awaitingPlay = true;
    }
}