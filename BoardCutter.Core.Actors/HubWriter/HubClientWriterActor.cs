using Akka.Actor;

using Microsoft.AspNetCore.SignalR;

using Newtonsoft.Json;

namespace BoardCutter.Core.Actors.HubWriter;

public class HubClientWriterActor<T> : ReceiveActor where T : Hub
{
    private readonly IHubContext<T> _hubContext;

    public HubClientWriterActor(IHubContext<T> hubContext)
    {
        _hubContext = hubContext;

        ReceiveAsync<HubWriterActorMessages.WriteClient>(ClientWrite);
        ReceiveAsync<HubWriterActorMessages.WriteAll>(AllWrite);
        ReceiveAsync<HubWriterActorMessages.WriteGroup>(GroupWrite);
        ReceiveAsync<HubWriterActorMessages.WriteGroupObject>(GroupWriteObject);
        ReceiveAsync<HubWriterActorMessages.AddToGroup>(AddToGroup);
        ReceiveAsync<HubWriterActorMessages.RemoveFromGroup>(RemoveFromGroup);
    }

    private async Task GroupWriteObject(HubWriterActorMessages.WriteGroupObject message)
    {
        await _hubContext.Clients.Group(message.GroupId)
            .SendAsync(message.Message, JsonConvert.SerializeObject(message.Payload));
    }

    private async Task AddToGroup(HubWriterActorMessages.AddToGroup message)
    {
        await _hubContext.Groups.AddToGroupAsync(message.ConnectionId, message.GroupId);
    }

    private async Task RemoveFromGroup(HubWriterActorMessages.RemoveFromGroup message)
    {
        await _hubContext.Groups.RemoveFromGroupAsync(message.ConnectionId, message.GroupId);
    }

    private async Task GroupWrite(HubWriterActorMessages.WriteGroup message)
    {
        await _hubContext.Clients.Group(message.GroupId).SendAsync(message.Message, message.Payload);
    }

    private async Task AllWrite(HubWriterActorMessages.WriteAll message)
    {
        await _hubContext.Clients.All.SendAsync(message.Message, message.Payload);
    }

    private async Task ClientWrite(HubWriterActorMessages.WriteClient message)
    {
        await _hubContext.Clients.Client(message.ConnectionId).SendAsync(message.Message, message.Payload);
    }
}