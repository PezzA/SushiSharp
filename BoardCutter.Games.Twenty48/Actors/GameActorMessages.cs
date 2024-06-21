using BoardCutter.Core.Players;

namespace BoardCutter.Games.Twenty48.Actors;

public class GameActorMessages
{
    public record SetupGameRequest(Player Player, int GridSize);

    public record CreateGameRequest(Player Player);

    public record StartGameRequest(Player Player);

    public record MoveRequest(Player Player, Direction Direction);

    public record GameCreated(PublicVisible GameData);
}

public enum Direction
{
    Up,
    Down,
    Left, 
    Right
}