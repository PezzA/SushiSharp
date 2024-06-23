using Akka.Actor;

using BoardCutter.Core.Players;

namespace BoardCutter.Core.Actors;

public class GameManagerMessages
{
    public record GetGameRequest(string GameId);

    public record GetGameDetails(IActorRef GameActor);

    public record CreateGameRequest(Player Player, string GameTag);

    public record CreateGameSpecificRequest(Player Player, string GameId);

    public record JoinGameRequest(Player Player, string GameId);

    public record GetGameList;
}