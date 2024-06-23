using BoardCutter.Core.Players;

namespace BoardCutter.Core.Actors;

public abstract class HubWriterMessages
{
    public record WriteClient(string ConnectionId, string Message, string Payload);
    
    public record WriteClientObject(Player Player, string Message, object Payload);

    public record WriteAll(string Message, string Payload);

    public record WriteGroupObject(string GroupId, string Message, object Payload);
}