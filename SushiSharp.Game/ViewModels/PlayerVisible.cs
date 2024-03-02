using SushiSharp.Cards;

namespace SushiSharp.Game.ViewModels;

public class PlayerVisible 
{
    public string PlayerId { get; set; } = string.Empty;
    public List<Card> Hand { get; set; } = [];
    public List<Card> CurrentPlay { get; set; } = [];
}