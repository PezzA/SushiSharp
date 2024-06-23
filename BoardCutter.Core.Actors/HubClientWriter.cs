using Akka.Actor;

using BoardCutter.Core.Players;

using Microsoft.AspNetCore.SignalR;

using Newtonsoft.Json;

namespace BoardCutter.Core.Actors.HubWriter;

public class HubClientWriter<T> : ReceiveActor where T : Hub
{
    private readonly IHubContext<T> _hubContext;
    private readonly IPlayerService _playerService;

    public HubClientWriter(IHubContext<T> hubContext, IPlayerService playerService)
    {
        _hubContext = hubContext;
        _playerService = playerService;

        ReceiveAsync<HubWriterMessages.WriteClient>(ClientWrite);
        ReceiveAsync<HubWriterMessages.WriteClientObject>(ClientWriteObject);
        ReceiveAsync<HubWriterMessages.WriteAll>(AllWrite);
        ReceiveAsync<HubWriterMessages.WriteGroupObject>(GroupWriteObject);
    }

    private async Task ClientWriteObject(HubWriterMessages.WriteClientObject message)
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

    private async Task GroupWriteObject(HubWriterMessages.WriteGroupObject message)
    {
        await _hubContext.Clients.Group(message.GroupId)
            .SendAsync(message.Message, JsonConvert.SerializeObject(message.Payload));
    }

    private async Task AllWrite(HubWriterMessages.WriteAll message)
    {
        await _hubContext.Clients.All.SendAsync(message.Message, message.Payload);
    }

    private async Task ClientWrite(HubWriterMessages.WriteClient message)
    {
        await _hubContext.Clients.Client(message.ConnectionId).SendAsync(message.Message, message.Payload);
    }
    
    private async Task ClientWriteObject(HubWriterMessages.WriteClient message)
    {
        await _hubContext.Clients.Client(message.ConnectionId).SendAsync(message.Message, message.Payload);
    }
}