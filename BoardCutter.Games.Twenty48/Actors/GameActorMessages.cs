namespace BoardCutter.Games.Twenty48.Actors;

public class GameNotifications
{
    public record GameUpdated(PublicVisible GameData);
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}