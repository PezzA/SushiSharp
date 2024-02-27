using SushiSharp.Game;
using SushiSharp.Game.ViewModels;

namespace SushiSharp.Web.Actors;

public class GameActorMessages
{
    public abstract record GameNotification(PublicGameData GameData);

    public abstract record PlayerRequest(Player Player);

    public abstract record PlayerGameRequest(Player Player, string GameId) : PlayerRequest(Player);

    public record GetGameListRequest(Player player) : PlayerRequest(player);

    public record CreateGameRequest(Player player) : PlayerRequest(player);

    public record StartGameRequest(Player player, string gameId) : PlayerGameRequest(player, gameId);

    public record JoinGameRequest(Player player, string gameId) : PlayerGameRequest(player, gameId);

    public record LeaveGameRequest(Player player, string gameId) : PlayerGameRequest(player, gameId);

    public record UpdateGameNotification(PublicGameData gameData) : GameNotification(gameData);
}
