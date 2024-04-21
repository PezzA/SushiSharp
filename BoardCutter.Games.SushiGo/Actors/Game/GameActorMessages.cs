using BoardCutter.Core.Players;
using BoardCutter.Games.SushiGo.Models;

namespace BoardCutter.Games.SushiGo.Actors.Game;

public abstract class GameActorMessages
{
    public abstract record GameNotification(PublicVisible GameData);

    public abstract record PlayerRequest(Player Player);

    public abstract record PlayerGameRequest(Player Player, string GameId) : PlayerRequest(Player);

    public record GetGameListRequest(Player Player) : PlayerRequest(Player);

    public record CreateGameRequest(Player Player, string GameTag, string? GameId = null) : PlayerRequest(Player);

    public record StartGameRequest(Player Player, string GameId) : PlayerGameRequest(Player, GameId);

    public record JoinGameRequest(Player Player, string GameId) : PlayerGameRequest(Player, GameId);

    public record LeaveGameRequest(Player Player, string GameId) : PlayerGameRequest(Player, GameId);


    public record GameCreated(PublicVisible GameData) : GameNotification(GameData);
    public record GameUpdated(PublicVisible GameData) : GameNotification(GameData);
    public record GameEnded(string GameId);

    public record GamePlayRequest(Player Player, string GameId, List<Card>? Played);
}
