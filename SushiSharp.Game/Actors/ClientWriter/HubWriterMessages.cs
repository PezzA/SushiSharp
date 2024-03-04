namespace SushiSharp.Game.Actors.ClientWriter;

public abstract class ClientWriterActorMessages
{
    public record WriteClient(string ConnectionId, string Message, string Payload);

    public record WriteAll(string Message, string Payload);

    public record WriteGroup(string GroupId, string Message, string Payload);

    public record AddToGroup(string GroupId, string ConnectionId);

    public record RemoveFromGroup(string GroupId, string ConnectionId);
}