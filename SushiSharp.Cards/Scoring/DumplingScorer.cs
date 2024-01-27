namespace SushiSharp.Cards.Scoring;

public class DumplingScorer : IScorer
{
    public CardType GetCardType { get => CardType.Dumpling; }

    public int Score(IList<Card> tableau)
    {
        var dumpings = tableau
            .Where(c => c.Type == GetCardType)
            .ToArray();

        return dumpings.Length switch
        {
            0 => 0,
            1 => 1,
            2 => 3,
            3 => 6,
            4 => 10,
            _ => 15
        };
    }
}