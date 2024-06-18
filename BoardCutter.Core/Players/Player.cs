using SpookilySharp;

namespace BoardCutter.Core.Players;

public class Player
{
    public Player(string connectionId, string userName, string id = "") : base()
    {
        Id = string.IsNullOrEmpty(id)
            ? Guid.NewGuid().ToString()
            : id;
        ConnectionId = connectionId;
        Name = userName;
    }

    public string Id { get; set; }

    public string ConnectionId { get; set; } = String.Empty;

    public string Name { get; set; } = string.Empty;

    public string AvatarPath() => Name == String.Empty
        ? string.Empty
        : $"https://api.dicebear.com/6.x/bottts-neutral/svg?seed={Name.SpookyHash64()}&size=32";
}