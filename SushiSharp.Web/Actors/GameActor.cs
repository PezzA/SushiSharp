using Akka.Actor;

using Newtonsoft.Json;

using SushiSharp.Cards;
using SushiSharp.Cards.Decks;
using SushiSharp.Cards.Shufflers;
using SushiSharp.Game;

namespace SushiSharp.Web.Actors;

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

    // Internal State
    private GameState _gameState;

    public GameActor(ICardShuffler cardShuffler, IActorRef hubWriterActor, Player creator, string gameId)
    {
        _cardShuffler = cardShuffler;
        _hubWriterActor = hubWriterActor;
        _creator = creator;
        _gameId = gameId;

        _gameState = new GameState(creator, gameId);
        Receive<GameActorMessages.StartGameRequest>(StartGame);
        Receive<GameActorMessages.JoinGameRequest>(JoinGame);
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

    private void JoinGame(GameActorMessages.JoinGameRequest message)
    {
        if (_gameState.GameData.Players.Count >= _gameState.GameData.Parameters.MaxPlayers)
        {
            _hubWriterActor.Tell(new HubWriterMessages.WriteClient(message.player.ConnectionId,
                ServerMessages.ErrorMessage, "Game was already full"));
            return;
        }

        if (_gameState.GameData.Players.Any(p => p.Id == message.player.Id))
        {
            _hubWriterActor.Tell(new HubWriterMessages.WriteClient(message.player.ConnectionId,
                ServerMessages.ErrorMessage, "Client is already in game?!"));
            return;
        }

        _gameState.GameData.Players.Add(message.player);

        _hubWriterActor.Tell(new HubWriterMessages.AddToGroup(message.gameId, message.player.ConnectionId));

        _hubWriterActor.Tell(new HubWriterMessages.WriteGroup(
            _gameState.GameData.Id,
            ServerMessages.SetGame,
            JsonConvert.SerializeObject(_gameState.GameData)));
        
        Context.Parent.Tell(new GameActorMessages.UpdateGameNotification(_gameState.GameData));
    }


    private void StartGame(GameActorMessages.StartGameRequest message)
    {
        // TODO - Only the person who created the game should be able to start it.

        _gameState.GameData.Status = GameStatus.Running;
        Context.Parent.Tell(new GameActorMessages.UpdateGameNotification(_gameState.GameData));

        _gameState.GameDeck = new Deck(_cardShuffler, SushiGoClassic.GetDeck());
        _gameState.GameDeck.Shuffle();

        _gameState.PlayerBoardStates = _gameState.GameData.Players
            .Select(p => new Tableau(p.Id, [], [], []))
            .ToList();

        for (int i = 0; i < 9; i++)
        {
            foreach (var boardState in _gameState.PlayerBoardStates)
            {
                (List<Card> cards, _) = _gameState.GameDeck.Draw(1);

                // not going to check for end of deck
                boardState.Hand.Add(cards[0]);
            }
        }

        _hubWriterActor.Tell(new HubWriterMessages.WriteGroup(_gameState.GameData.Id, ServerMessages.SetGame,
            JsonConvert.SerializeObject(_gameState.GameData)));
    }
}