using Akka.Actor;
using Akka.Hosting;

using BoardCutter.Core.Actors;
using BoardCutter.Core.Players;
using BoardCutter.Games.SushiGo;
using BoardCutter.Games.SushiGo.Actors.Game;

using Microsoft.AspNetCore.SignalR;

using Newtonsoft.Json;

namespace BoardCutter.Web.Hubs;

public class SushiHub(
    IRequiredActor<GameManager> gameManagerActor,
    IPlayerService playerService) : Hub
{
    private readonly IActorRef _gameManagerActor = gameManagerActor.ActorRef;

    public async Task SubmitTurn(string gameId, string payload)
    {
        var cards = JsonConvert.DeserializeObject<List<Card>>(payload);

        var player = await playerService.GetPlayerByConnectionId(Context.ConnectionId);

        if (player == null)
        {
            return;
        }

        _gameManagerActor.Tell(new GameActorMessages.GamePlayRequest(player, gameId, cards));
    }

    public Task LeaveGame(string gameId) => GenericPlayerGameRequest<GameActorMessages.LeaveGameRequest>(gameId);

    public Task StartGame(string gameId) => GenericPlayerGameRequest<GameActorMessages.StartGameRequest>(gameId);

    private async Task GenericPlayerGameRequest<T>(string gameId) where T : GameActorMessages.PlayerGameRequest
    {
        var player = await playerService.GetPlayerByConnectionId(Context.ConnectionId);

        if (player == null)
        {
            return;
        }

        var message = (T)Activator.CreateInstance(typeof(T), player, gameId)!;

        _gameManagerActor.Tell(message);
    }

    // Someone landing on an instance of a game, should be created at this point
    public async Task InitClientGame(string gameId)
    {
        var loggedInUserName = Context.User?.Identity?.Name;

        if (string.IsNullOrEmpty(loggedInUserName)) throw new InvalidDataException("Should have a user");

        var player = await playerService.AddOrUpdatePlayer(loggedInUserName, Context.ConnectionId, true);

        if (player == null) throw new InvalidDataException("Could not find user");

        await Clients.Caller.SendAsync("SetIdentity", player.Id);
    }
}