using Akka.Actor;
using Akka.Hosting;

using BoardCutter.Core.Actors;
using BoardCutter.Core.Players;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BoardCutter.Web.Hubs;

public record GetBasicDetailsResult(bool Success, Player? Player, IActorRef? GameActor);

[Authorize]
public class Twenty48Hub(IRequiredActor<GameManager> gameManagerActor, IPlayerService playerService) : Hub
{
    private readonly IActorRef _gameManagerActor = gameManagerActor.ActorRef;

    private async Task<GetBasicDetailsResult> GetBasicDetails(string gameId)
    {
        var player = await playerService.GetPlayerByConnectionId(Context.ConnectionId);

        if (player == null)
        {
            await Clients.Caller.SendAsync("Could not find player connection");
            return new GetBasicDetailsResult(false, null, null);
        }

        if (await _gameManagerActor.Ask(new GameManagerMessages.GetGameRequest(gameId),
                TimeSpan.FromMilliseconds(100)) is not
            GameManagerMessages.GetGameDetails gameActorResp)
        {
            await Clients.Caller.SendAsync("Could not find requested game");
            return new GetBasicDetailsResult(false, null, null);
        }

        return new GetBasicDetailsResult(true, player, gameActorResp.GameActor);
    }

    public async Task SetupGame(string gameId, int size)
    {
        var requestDetails = await GetBasicDetails(gameId);

        if (requestDetails is not { Success: true } ||
            requestDetails.Player is null ||
            requestDetails.GameActor is null)
        {
            return;
        }

        requestDetails.GameActor.Tell(
            new Games.Twenty48.Actors.GameMessages.SetupGameRequest(requestDetails.Player, size));
    }
}