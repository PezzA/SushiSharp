using SushiSharp.Cards;
using SushiSharp.Game.ViewModels;

namespace SushiSharp.Game;

public class GameState(Player creator)
{
    public PublicGameData GameData { get; set; } = new()
    {
        Players = [creator],
        Id = Guid.NewGuid().ToString(),
        Status = GameStatus.SettingUp,
        Parameters = new GameParameters(2)
    };

    public Deck? GameDeck { get; set; } = null;

    public List<Card> DiscardPile { get; set; } = [];

    public List<Tableau> PlayerBoardStates { get; set; } = [];

    public PublicPlayerData GetPublicDataForPlayer(string playerId)
    {
        return new PublicPlayerData
        {
            Hand = PlayerBoardStates.Single(pbs => pbs.PlayerId == playerId).Hand.ToArray(),
            Opponents = PlayerBoardStates.ToDictionary(
                o => o.PlayerId,
                o => new Opponent
                {
                    Played = o.Played.ToArray(), Sideboard = o.Side.ToArray(), HandSize = o.Hand.Count
                })
        };
    }
}