namespace BoardCutter.Games.SushiGo.Models;

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