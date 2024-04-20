namespace BoardCutter.Core.Actors.HubWriter;

public abstract class HubWriterActorMessages
{
    public record WriteClient(string ConnectionId, string Message, string Payload);

    public record WriteAll(string Message, string Payload);

    public record WriteGroup(string GroupId, string Message, string Payload);
    public record WriteGroupObject(string GroupId, string Message, object Payload);

    public record AddToGroup(string GroupId, string ConnectionId);

    public record RemoveFromGroup(string GroupId, string ConnectionId);
}