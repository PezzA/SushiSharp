﻿using Akka.Actor;

using BoardCutter.Core.Actors.HubWriter;
using BoardCutter.Games.SushiGo.Decks;
using BoardCutter.Games.SushiGo.Models;
using BoardCutter.Games.SushiGo.Players;
using BoardCutter.Games.SushiGo.Scoring;
using BoardCutter.Games.SushiGo.Shufflers;

using Newtonsoft.Json;

namespace BoardCutter.Games.SushiGo.Actors.Game;

/// <summary>
/// GameActor holds all the state for an individual game
/// </summary>
public class GameActor : ReceiveActor
{
    // Dependencies
    private readonly ICardShuffler _cardShuffler;
    private readonly Dictionary<CardType, IScorer> _scorers;
    private readonly IActorRef _hubWriterActor;

    // Board State

    /// <summary>
    /// Unique game identifier, can be passed in, or will be generated by default.
    /// </summary>
    private readonly string _gameId;

    /// <summary>
    /// Overall game status
    /// </summary>
    private GameStatus _status = GameStatus.SettingUp;

    /// <summary>
    /// Game parameters
    /// </summary>
    private GameParameters _parameters = new GameParameters(2);

    /// <summary>
    /// List of current players in the game
    /// </summary>
    private List<Player> _players = [];

    /// <summary>
    /// Player who created the game
    /// </summary>
    private readonly Player _creator;

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

    // looks deep, but the game is scored, by player, cardtype and round.
    private readonly Dictionary<int, Dictionary<CardType, Dictionary<string, int>>> _gameScores = [];

    private readonly Dictionary<string, int> _finalScores = [];

    public GameActor(ICardShuffler cardShuffler, Dictionary<CardType, IScorer> scorers, IActorRef hubWriterActor,
        Player creator, string? gameId = null)
    {
        _cardShuffler = cardShuffler;
        _scorers = scorers;
        _hubWriterActor = hubWriterActor;
        _creator = creator;

        _gameId = string.IsNullOrWhiteSpace(gameId)
            ? Guid.NewGuid().ToString()
            : gameId;

        _players = [creator];

        Receive<GameActorMessages.StartGameRequest>(StartGame);
        Receive<GameActorMessages.CreateGameRequest>(CreateGame);
        Receive<GameActorMessages.JoinGameRequest>(JoinGame);
        Receive<GameActorMessages.GamePlayRequest>(GamePlay);
        Receive<GameActorMessages.LeaveGameRequest>(LeaveGame);
    }

    private void LeaveGame(GameActorMessages.LeaveGameRequest message)
    {
        var playerIndex = _players.FindIndex(p => p.Id == message.Player.Id);

        if (playerIndex == -1)
        {
            SendError(message.Player.ConnectionId, "You are not a member of the game you just tried to leave?");
            return;
        }

        if (_status == GameStatus.Running)
        {
            SendError(message.Player.ConnectionId, "You cannot leave a game that is running.");
            return;
        }

        _players.RemoveAt(playerIndex);

        _hubWriterActor.Tell(new ClientWriterActorMessages.RemoveFromGroup(_gameId, message.Player.ConnectionId));
        _hubWriterActor.Tell(new ClientWriterActorMessages.WriteClient(message.Player.ConnectionId,
            ServerMessages.SetPlayerGame, string.Empty));

        if ((message.Player.Id == _creator.Id && _status == GameStatus.SettingUp) || _players.Count == 0)
        {
            foreach (var player in _players)
            {
                _hubWriterActor.Tell(
                    new ClientWriterActorMessages.RemoveFromGroup(_gameId, player.ConnectionId));
                _hubWriterActor.Tell(new ClientWriterActorMessages.WriteClient(player.ConnectionId,
                    ServerMessages.SetPlayerGame, string.Empty));
            }


            NotifyGameEnded();
        }
    }

    private PublicVisible GetPublicVisibleData() => new PublicVisible
    {
        Id = _gameId, Players = _players, Parameters = _parameters, Status = _status
    };

    private void CreateGame(GameActorMessages.CreateGameRequest message)
    {
        var publicVisibleData = GetPublicVisibleData();
        Sender.Tell(new GameActorMessages.UpdateGameNotification(publicVisibleData));

        _hubWriterActor.Tell(new ClientWriterActorMessages.AddToGroup(_gameId, _creator.ConnectionId));

        _hubWriterActor.Tell(new ClientWriterActorMessages.WriteClient(
            _creator.ConnectionId,
            ServerMessages.SetPlayerGame,
            JsonConvert.SerializeObject(publicVisibleData)));
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
        var playerIndexes = _players
            .Select((player, index) => (index, player))
            .ToDictionary(kp => kp.index, kp => kp.player.Id);

        var memoHand = _playerBoardStates[playerIndexes[0]].Hand;

        for (int i = 0; i < _players.Count - 1; i++)
        {
            _playerBoardStates[playerIndexes[i]].Hand = _playerBoardStates[playerIndexes[i + 1]].Hand;
        }

        _playerBoardStates[playerIndexes[_players.Count - 1]].Hand = memoHand;
    }

    private bool AllCardsPlayed()
    {
        return _playerBoardStates[_creator.Id].Hand.Count == 0;
    }

    private void ScoreGame()
    {
        var puddingScores = _scorers
            .Single(ct => ct.Key == CardType.Pudding)
            .Value
            .Score(_playerBoardStates.Select(kv => kv.Value).ToList());

        var roundScore = new Dictionary<CardType, Dictionary<string, int>> { { CardType.Pudding, puddingScores } };

        _gameScores.Add(4, roundScore);

        foreach (var player in _players)
        {
            _finalScores.Add(player.Id, _gameScores.Sum(r => r.Value.Sum(ct => ct.Value[player.Id])));
        }
    }

    private void UpdateGameState()
    {
        // take all the plays and process hands
        ProcessPlays();

        if (!AllCardsPlayed())
        {
            RotateHands();
            return;
        }

        ScoreRound();
        ProcessHands();

        if (_round == 3)
        {
            ScoreGame();
            SetGameStatus(GameStatus.Results);
            BroadcastPublicVisibleData();
            BroadcastViewerVisibleData();
            NotifyGameChanged();
            return;
        }

        DealCards();
        BroadcastViewerVisibleData();
        _round += 1;
    }

    private void ScoreRound()
    {
        var roundScores = new Dictionary<CardType, Dictionary<string, int>>();

        foreach ((CardType cardType, IScorer scorer) in _scorers.Where(s => s.Key != CardType.Pudding))
        {
            roundScores.Add(cardType, scorer.Score(_playerBoardStates.Select(kp => kp.Value).ToList()));
        }

        _gameScores.Add(_round, roundScores);
    }

    private void ProcessHands()
    {
        foreach ((_, Tableau boardState) in _playerBoardStates)
        {
            // deserts to the side board
            boardState.Side.AddRange(boardState.Played.Where(c => c.Type == CardType.Pudding));

            // everything else to the discard pile
            _discardPile.AddRange(boardState.Played.Where(c => c.Type != CardType.Pudding));

            // empty the played cards
            boardState.Played = [];
        }
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

        if (_playerTurnList.Count == _parameters.MaxPlayers)
        {
            UpdateGameState();

            _playerTurnList = [];

            BroadcastViewerVisibleData();
            BroadCastPlayerVisibleData();
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
        if (_players.Count >= _parameters.MaxPlayers)
        {
            SendError(message.Player.ConnectionId, "Max players in game.");
            return;
        }

        if (_players.Any(p => p.Id == message.Player.Id))
        {
            SendError(message.Player.ConnectionId, "Client is already in a game.");
            return;
        }

        _players.Add(message.Player);

        _hubWriterActor.Tell(new ClientWriterActorMessages.AddToGroup(
            message.GameId,
            message.Player.ConnectionId));

        _hubWriterActor.Tell(new ClientWriterActorMessages.WriteGroup(
            _gameId,
            ServerMessages.SetPlayerGame,
            JsonConvert.SerializeObject(GetPublicVisibleData())));

        Sender.Tell(new GameActorMessages.UpdateGameNotification(GetPublicVisibleData()));
    }

    private void DealCards()
    {
        if (_drawPile == null) throw new InvalidOperationException("No game state or game Deck");

        for (int i = 0; i < 10; i++)
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
        var statii = _players.ToDictionary(p => p.Id, p => _playerTurnList.ContainsKey(p.Id));

        _hubWriterActor.Tell(new ClientWriterActorMessages.WriteGroup(
            _gameId,
            ServerMessages.SetPlayerTurnStatus,
            JsonConvert.SerializeObject(statii)));
    }

    private void BroadCastPlayerVisibleData()
    {
        foreach (var player in _players)
        {
            _hubWriterActor.Tell(new ClientWriterActorMessages.WriteClient(
                player.ConnectionId,
                ServerMessages.SetPlayerVisibleData,
                JsonConvert.SerializeObject(GetPlayerVisibleData(player.Id))));
        }
    }

    private void SendFullBoardState()
    {
        BroadcastPublicVisibleData();
        BroadcastViewerVisibleData();
        BroadCastPlayerVisibleData();
        SendPlayerStatus();
    }

    private void BroadcastPublicVisibleData()
    {
        _hubWriterActor.Tell(new ClientWriterActorMessages.WriteGroup(
            _gameId,
            ServerMessages.SetPlayerGame,
            JsonConvert.SerializeObject(GetPublicVisibleData())));
    }

    private void BroadcastViewerVisibleData()
    {
        _hubWriterActor.Tell(new ClientWriterActorMessages.WriteGroup(
            _gameId,
            ServerMessages.SetViewerVisibleData,
            JsonConvert.SerializeObject(GetViewerVisibleData())));
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
                }),
            GameScores = _gameScores,
            FinalScores = _finalScores
        };

    private void NotifyGameChanged() =>
        Context.Parent.Tell(new GameActorMessages.UpdateGameNotification(GetPublicVisibleData()));

    private void NotifyGameEnded() =>
        Context.Parent.Tell(new GameActorMessages.GameEndedNotification(_gameId));

    private void SetGameStatus(GameStatus status)
    {
        _status = status;
    }


    private void StartGame(GameActorMessages.StartGameRequest message)
    {
        if (!IsGameValidToStart(message))
        {
            return;
        }

        SetGameStatus(GameStatus.Running);

        foreach (var player in _players)
        {
            _playerBoardStates[player.Id] = new Tableau(player.Id, [], [], []);
        }

        _playerTurnList = [];

        NotifyGameChanged();

        ShuffleDeck();

        DealCards();

        SendFullBoardState();

        _awaitingPlay = true;
    }

    private bool IsGameValidToStart(GameActorMessages.StartGameRequest message)
    {
        if (message.Player.Id != _creator.Id)
        {
            SendError(message.Player.ConnectionId, "Only the person who created the game can start the game.");
            return false;
        }

        // TODO - Magic number
        if (_players.Count < 2)
        {
            SendError(message.Player.ConnectionId, "A game must have at least 2 players to begin.");
            return false;
        }

        return true;
    }
}