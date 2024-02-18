using SushiSharp.Cards;

namespace SushiSharp.Game.ViewModels;

public class Opponent
{
    public Card[] Played { get; set; } = [];
    public Card[] Sideboard { get; set; } = [];
    public int HandSize { get; set; }
}
