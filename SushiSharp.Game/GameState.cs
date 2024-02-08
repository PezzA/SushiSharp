using SushiSharp.Cards;

namespace SushiSharp.Game;

public class GameState(Player creator)
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public GameStatus Status { get; set; } = GameStatus.SettingUp;

    public GameParameters Parameters { get; set; } = new(2);

    public List<Player> Players { get; set; } = new() { creator };
}