using SushiSharp.Cards;

namespace SushiSharp.Game;

public class GameState(Player creatorPlayerId)
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public GameStatus Status { get; set; } = GameStatus.SettingUp;

    public GameParameters Parameters { get; set; } = new(2);

    public List<Player> Players { get; set; } = [creatorPlayerId];

    public List<Card> Deck { get; set; } = [];

}