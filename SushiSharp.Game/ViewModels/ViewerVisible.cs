namespace SushiSharp.Game.ViewModels;

public class ViewerVisible
{
    public Dictionary<string, OpponentState> OpponentStates = [];
    public int CardsInDeck { get; set; }
    public int CardsInDiscard { get; set; }
    public int RoundNumber { get; set; }
}
