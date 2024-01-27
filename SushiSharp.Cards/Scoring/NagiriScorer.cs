namespace SushiSharp.Cards.Scoring;

public class NagiriScorer : IScorer
{
    public CardType GetCardType { get => CardType.Nagiri; }

    public int Score(IList<Card> tableau)
    {
        var wasibiCount =  tableau.Where(c => c.Type == CardType.Wasabi).Count();
        
        var total = 0;
        foreach (var t in tableau)
        {
            if (t.Type == CardType.Nagiri)
            {
                var score = t.Symbols[0] switch
                {
                    CardSymbol.NagiriEgg => 1,
                    CardSymbol.NagiriSalmon => 2,
                    CardSymbol.NagiriSquid => 3,
                    _ => throw new ArgumentOutOfRangeException(
                        $"No Symbols, or unexpected symbol on Nagiri card. {t.Symbols}")
                };

                if (wasibiCount > 0)
                {
                    score *= 3;
                    wasibiCount -= 1;
                }

                total += score;
            }

            return total;
        }
    }
}