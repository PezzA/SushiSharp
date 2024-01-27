namespace SushiSharp.Cards.Scoring;

public class SashimiScorer : IScorer
{
    public CardType GetCardType { get => CardType.Sashimi; }

    public int Score(IList<Card> tableau)
    {
        var sashimiCards = tableau.Where(c => c.Type == GetCardType).ToArray();
        return (sashimiCards.Length / 3) * 10;
    }
}