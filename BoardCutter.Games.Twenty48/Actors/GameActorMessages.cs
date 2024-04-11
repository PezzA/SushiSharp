using BoardCutter.Games.SushiGo.Players;

namespace BoardCutter.Games.Twenty48.Actors;

public class GameActorMessages
{
    public abstract record SetupGameRequest(Player Player, int GridSize);

    public abstract record CreateGameRequest(Player Player);

    public abstract record StartGameRequest(Player Player);

    public abstract record MoveRequest(Player Player, Direction Direction);
}

public enum Direction
{
    Up,
    Down,
    Left, 
    Right
}