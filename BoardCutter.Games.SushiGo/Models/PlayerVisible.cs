namespace BoardCutter.Games.SushiGo.Models;

public class PlayerVisible 
{
    public string PlayerId { get; set; } = string.Empty;
    public List<Card> Hand { get; set; } = [];
    public List<Card> CurrentPlay { get; set; } = [];
}