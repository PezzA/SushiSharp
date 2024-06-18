using Akka.Actor;

using BoardCutter.Core.Players;

using Microsoft.AspNetCore.SignalR;

using Newtonsoft.Json;

namespace BoardCutter.Core.Actors.HubWriter;

public class HubClientWriterActor<T> : ReceiveActor where T : Hub
{
    private readonly IHubContext<T> _hubContext;
    private readonly IPlayerService _playerService;

    public HubClientWriterActor(IHubContext<T> hubContext, IPlayerService playerService)
    {
        _hubContext = hubContext;
        _playerService = playerService;

        ReceiveAsync<HubWriterActorMessages.WriteClient>(ClientWrite);
        ReceiveAsync<HubWriterActorMessages.WriteClientObject>(ClientWriteObject);
        ReceiveAsync<HubWriterActorMessages.WriteAll>(AllWrite);
        ReceiveAsync<HubWriterActorMessages.WriteGroupObject>(GroupWriteObject);
    }

    private async Task ClientWriteObject(HubWriterActorMessages.WriteClientObject message)
    {
        var player = await _playerService.GetPlayerByUser(message.Player.Name);

        if (player == null)
        {
            // Should log
            return;
        }

        await _hubContext.Clients.Client(player.ConnectionId)
            .SendAsync(message.Message, JsonConvert.SerializeObject(message.Payload));
    }

    private async Task GroupWriteObject(HubWriterActorMessages.WriteGroupObject message)
    {
        await _hubContext.Clients.Group(message.GroupId)
            .SendAsync(message.Message, JsonConvert.SerializeObject(message.Payload));
    }

    private async Task AllWrite(HubWriterActorMessages.WriteAll message)
    {
        await _hubContext.Clients.All.SendAsync(message.Message, message.Payload);
    }

    private async Task ClientWrite(HubWriterActorMessages.WriteClient message)
    {
        await _hubContext.Clients.Client(message.ConnectionId).SendAsync(message.Message, message.Payload);
    }
    
    private async Task ClientWriteObject(HubWriterActorMessages.WriteClient message)
    {
        await _hubContext.Clients.Client(message.ConnectionId).SendAsync(message.Message, message.Payload);
    }
}