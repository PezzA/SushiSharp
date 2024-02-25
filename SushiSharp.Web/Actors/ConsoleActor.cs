using Akka.Actor;
using Akka.Event;

using Microsoft.AspNetCore.SignalR;

using SushiSharp.Web.Hubs;

namespace SushiSharp.Web.Actors;

public sealed class ConsoleActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    public ConsoleActor(IHubContext<LobbyHub> hubContext)
    {
        ReceiveAnyAsync(async o => 
        {
            _log.Info("Received: {0}", o);
            await hubContext.Clients.All.SendAsync("DebugMessage", "This is a debug message");
        });
    }
}