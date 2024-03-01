using Akka.Actor;

using Microsoft.AspNetCore.SignalR;

using SushiSharp.Web.Hubs;

namespace SushiSharp.Web.Actors.HubWriter;

public class HubWriterActor : ReceiveActor
{
    private readonly IHubContext<LobbyHub> _hubContext;

    public HubWriterActor(IHubContext<LobbyHub> hubContext)
    {
        _hubContext = hubContext;

        ReceiveAsync<HubWriterMessages.WriteClient>(ClientWrite);
        ReceiveAsync<HubWriterMessages.WriteAll>(AllWrite);
        ReceiveAsync<HubWriterMessages.WriteGroup>(GroupWrite);
        ReceiveAsync<HubWriterMessages.AddToGroup>(AddToGroup);
        ReceiveAsync<HubWriterMessages.RemoveFromGroup>(RemoveFromGroup);
    }

    private async Task AddToGroup(HubWriterMessages.AddToGroup message)
    {
        await _hubContext.Groups.AddToGroupAsync(message.ConnectionId, message.GroupId);
    }

    private async Task RemoveFromGroup(HubWriterMessages.RemoveFromGroup message)
    {
        await _hubContext.Groups.RemoveFromGroupAsync(message.ConnectionId, message.GroupId);
    }

    private async Task GroupWrite(HubWriterMessages.WriteGroup message)
    {
        await _hubContext.Clients.Group(message.GroupId).SendAsync(message.Message, message.Payload);
    }

    private async Task AllWrite(HubWriterMessages.WriteAll message)
    {
        await _hubContext.Clients.All.SendAsync(message.Message, message.Payload);
    }

    private async Task ClientWrite(HubWriterMessages.WriteClient message)
    {
        await _hubContext.Clients.Client(message.ConnectionId).SendAsync(message.Message, message.Payload);
    }
}