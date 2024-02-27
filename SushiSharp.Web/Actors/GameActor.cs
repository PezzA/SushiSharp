using Akka.Actor;

using Microsoft.AspNetCore.SignalR;

using Newtonsoft.Json;

using SushiSharp.Cards;
using SushiSharp.Cards.Decks;
using SushiSharp.Cards.Shufflers;
using SushiSharp.Game;
using SushiSharp.Game.ViewModels;
using SushiSharp.Web.Hubs;

namespace SushiSharp.Web.Actors;

/// <summary>
/// GameActor holds all the state for an individual game
/// </summary>
public class GameActor : ReceiveActor
{
    // Dependencies
    private readonly IHubContext<LobbyHub> _hubContext;
    private readonly Player _creator;
    private readonly string _gameId;
    private readonly ICardShuffler _cardShuffler;
        
    // Internal State
    private PublicGameData _publicGameData;
    private Deck? _gameDeck = null;
    private List<Card> _discardPile = [];
    private List<Tableau> _playerBoardStates = [];

    public GameActor(IHubContext<LobbyHub> hubContext, ICardShuffler cardShuffler, Player creator, string gameId)
    {
        _hubContext = hubContext;
        _cardShuffler = cardShuffler;
        _creator = creator;
        _gameId = gameId;

        ReceiveAsync<GameActorMessages.StartGameRequest>(StartGame);
    }

    protected override void PreStart()
    {
        _publicGameData = new PublicGameData { Players = [_creator], Id = _gameId, };

        Context.Parent.Tell(new GameActorMessages.UpdateGameNotification(_publicGameData));
    }
    
    private async Task StartGame(GameActorMessages.StartGameRequest message)
    {
        // TODO - Only the person who created the game should be able to start it.
        
        
        _publicGameData.Status = GameStatus.Running;
        Context.Parent.Tell(new GameActorMessages.UpdateGameNotification(_publicGameData));

        _gameDeck = new Deck(_cardShuffler, SushiGoClassic.GetDeck());
        _gameDeck.Shuffle();

        _publicGameData.Status = GameStatus.Running;

        _playerBoardStates = _publicGameData.Players
            .Select(p => new Tableau(p.Id, [], [], []))
            .ToList();

        for (int i = 0; i < 9; i++)
        {
            foreach (var boardState in _playerBoardStates)
            {
                (List<Card> cards, _) = _gameDeck.Draw(1);

                // not going to check for end of deck
                boardState.Hand.Add(cards[0]);
            }
        }


        await _hubContext.Clients.Group(_publicGameData.Id)
            .SendAsync("SetGameData", JsonConvert.SerializeObject(_publicGameData));
    }
}