namespace BoardCutter.Games.SushiGo.Models;

public class ViewerVisible
{
    public Dictionary<string, OpponentState> OpponentStates = [];
    public int CardsInDeck { get; set; }
    public int CardsInDiscard { get; set; }
    public int RoundNumber { get; set; }

    public Dictionary<int, Dictionary<CardType, Dictionary<string, int>>> GameScores { get; set; } = [];

    public Dictionary<string, int> FinalScores { get; set; } = [];
}