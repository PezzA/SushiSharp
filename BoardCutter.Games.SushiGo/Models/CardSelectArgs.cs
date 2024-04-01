namespace BoardCutter.Games.SushiGo.Models;

public class CardSelectArgs
{
    public bool Selected { get; set; }
    public required Card Card { get; set; }
}