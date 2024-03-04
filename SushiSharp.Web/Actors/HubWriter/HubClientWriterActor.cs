using Akka.Actor;

using Microsoft.AspNetCore.SignalR;

using SushiSharp.Game.Actors.ClientWriter;
using SushiSharp.Web.Hubs;

namespace SushiSharp.Web.Actors.HubWriter;

public class HubClientWriterActor : ReceiveActor
{
    private readonly IHubContext<LobbyHub> _hubContext;

    public HubClientWriterActor(IHubContext<LobbyHub> hubContext)
    {
        _hubContext = hubContext;

        ReceiveAsync<ClientWriterActorMessages.WriteClient>(ClientWrite);
        ReceiveAsync<ClientWriterActorMessages.WriteAll>(AllWrite);
        ReceiveAsync<ClientWriterActorMessages.WriteGroup>(GroupWrite);
        ReceiveAsync<ClientWriterActorMessages.AddToGroup>(AddToGroup);
        ReceiveAsync<ClientWriterActorMessages.RemoveFromGroup>(RemoveFromGroup);
    }

    private async Task AddToGroup(ClientWriterActorMessages.AddToGroup message)
    {
        await _hubContext.Groups.AddToGroupAsync(message.ConnectionId, message.GroupId);
    }

    private async Task RemoveFromGroup(ClientWriterActorMessages.RemoveFromGroup message)
    {
        await _hubContext.Groups.RemoveFromGroupAsync(message.ConnectionId, message.GroupId);
    }

    private async Task GroupWrite(ClientWriterActorMessages.WriteGroup message)
    {
        await _hubContext.Clients.Group(message.GroupId).SendAsync(message.Message, message.Payload);
    }

    private async Task AllWrite(ClientWriterActorMessages.WriteAll message)
    {
        await _hubContext.Clients.All.SendAsync(message.Message, message.Payload);
    }

    private async Task ClientWrite(ClientWriterActorMessages.WriteClient message)
    {
        await _hubContext.Clients.Client(message.ConnectionId).SendAsync(message.Message, message.Payload);
    }
}