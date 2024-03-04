using SushiSharp.Game.Players;

namespace SushiSharp.Game.Models;

public class PublicVisible
{
    public string Id { get; set; } = string.Empty;

    public GameStatus Status{ get; set; }

    public GameParameters Parameters { get; set; } = null!;

    public List<Player> Players { get; set; } = [];
}