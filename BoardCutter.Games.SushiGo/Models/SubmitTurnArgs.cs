namespace BoardCutter.Games.SushiGo.Models;

public class SubmitTurnArgs
{
    public required string GameId { get; set; }
    public required List<Card> Cards { get; set; }
}