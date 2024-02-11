namespace SushiSharp.Cards.Scoring;

public class TempuraScorer : IScorer
{
    public Dictionary<string, int> Score(IList<Tableau> gameState)
    {
        var scores = new Dictionary<string, int>();

        foreach (var tab in gameState)
        {
            var sashimiCards = tab.Played
                .Where(c => c.Type == CardType.Tempura)
                .ToArray();

            scores.Add(tab.PlayerId, (sashimiCards.Length / 2) * 5);
        }

        return scores;
    }
}