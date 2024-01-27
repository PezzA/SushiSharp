namespace SushiSharp.Cards.Scoring;

public class TempuraScorer : IScorer
{
    public CardType GetCardType { get => CardType.Tempura; }

    public int Score(IList<Card> tableau)
    {
        var tempuraCards = tableau.Where(c => c.Type == GetCardType).ToArray();

        return (tempuraCards.Length / 2) * 5;
    }
}