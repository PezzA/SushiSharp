using SpookilySharp;

namespace SushiSharp.Game;

public class Player
{
    public Player()
    {
    }

    public Player(string connectionId, string userName) : base()
    {
        ConnectionId = connectionId;
        Name = userName;
    }

    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string ConnectionId { get; set; } = String.Empty;

    public string Name { get; set; } = string.Empty;

    public string AvatarPath() => Name == String.Empty
        ? string.Empty
        : $"https://api.dicebear.com/6.x/bottts-neutral/svg?seed={Name.SpookyHash64()}&size=32";
}