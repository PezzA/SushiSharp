using SushiSharp.Cards;
using SushiSharp.Game.ViewModels;

namespace SushiSharp.Game;

public class GameState(Player creator, string gameId)
{
    public PublicVisible GameData { get; set; } = new()
    {
        Players = [creator],
        Id = gameId,
        Status = GameStatus.SettingUp,
        Parameters = new GameParameters(2)
    };

    public Deck? GameDeck { get; set; } = null;

    public List<Card> DiscardPile { get; set; } = [];

    public List<Tableau> PlayerBoardStates { get; set; } = [];

    public PlayerVisible GetPublicDataForPlayer(string playerId)
    {
        return new PlayerVisible
        {
            PlayerId = playerId,
            Hand = PlayerBoardStates.Single(pbs => pbs.PlayerId == playerId).Hand,
        };
    }
}