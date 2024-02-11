namespace SushiSharp.Cards.Scoring;

public class NagiriScorer : IScorer
{
    public Dictionary<string, int> Score(IList<Tableau> gameState)
    {
        var scores = new Dictionary<string, int>();

        foreach (var tab in gameState)
        {
            var wasabiCount = tab.Played.Count(c => c.Type == CardType.Wasabi);

            var total = 0;

            // Ordering is important, scorer assumes cards are given in order played.  Playing a nagiri, will
            // auto dip an available wasabi.
            foreach (var card in tab.Played)
            {
                if (card.Type != CardType.Nagiri)
                {
                    continue;
                }

                var score = card.Symbols[0] switch
                {
                    CardSymbol.NagiriEgg => 1,
                    CardSymbol.NagiriSalmon => 2,
                    CardSymbol.NagiriSquid => 3,
                    _ => throw new ArgumentOutOfRangeException(
                        $"No Symbols, or unexpected symbol on Nagiri card. {card.Symbols}")
                };

                if (wasabiCount > 0)
                {
                    score *= 3;
                    wasabiCount -= 1;
                }

                total += score;
            }

            scores.Add(tab.PlayerId, total);
        }

        return scores;
    }
}