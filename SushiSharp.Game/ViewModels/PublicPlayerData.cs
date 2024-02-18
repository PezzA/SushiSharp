using SushiSharp.Cards;

namespace SushiSharp.Game.ViewModels;

public class PublicPlayerData
{
    public Dictionary<string, Opponent> Opponents { get; set; } = [];
    public Card[] Hand { get; set; } = [];
}