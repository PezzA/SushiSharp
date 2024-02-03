using SushiSharp.Cards;

namespace SushiSharp.Web.Model;

public enum GameStatus
{
    SettingUp = 0,
    Running = 1,
    Results = 2
}

public class GameState(Player creator)
{
    public GameStatus Status { get; set; } = GameStatus.SettingUp;

    public GameParameters Parameters { get; set; } = new(2);

    public List<Player> Players { get; set; } = new() {creator};
}

public class GameParameters(int maxPlayers)
{
    public int MaxPlayers { get; set; } = maxPlayers;
    public int MaxTurnTime { get; set; } = 20;
    public CardType? Rolls { get; set; }
    public CardType? Appetizer1 { get; set; }
    public CardType? Appetizer2 { get; set; }
    public CardType? Appetizer3 { get; set; }
    public CardType? Special1 { get; set; }
    public CardType? Special2 { get; set; }
    public CardType? Desert { get; set; }
}