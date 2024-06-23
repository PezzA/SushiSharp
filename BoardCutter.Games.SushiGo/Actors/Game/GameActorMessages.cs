using BoardCutter.Core.Players;

namespace BoardCutter.Games.SushiGo.Actors.Game;

public abstract class GameActorMessages
{
    public abstract record PlayerRequest(Player Player);

    public abstract record PlayerGameRequest(Player Player, string GameId) : PlayerRequest(Player);

    public record StartGameRequest(Player Player, string GameId) : PlayerGameRequest(Player, GameId);

    public record LeaveGameRequest(Player Player, string GameId) : PlayerGameRequest(Player, GameId);

    public record GamePlayRequest(Player Player, string GameId, List<Card>? Played);
}
