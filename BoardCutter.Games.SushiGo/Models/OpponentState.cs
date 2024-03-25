namespace BoardCutter.Games.SushiGo.Models;

public class OpponentState
{
    public List<Card> Played { get; set; } = [];
    public List<Card> Sideboard { get; set; } = [];
    public int HandSize { get; set; }
}
