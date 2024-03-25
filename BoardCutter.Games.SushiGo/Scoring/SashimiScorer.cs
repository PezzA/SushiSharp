namespace BoardCutter.Games.SushiGo.Scoring;

public class SashimiScorer : IScorer
{
    public Dictionary<string, int> Score(IList<Tableau> gameState)
    {
        var scores = new Dictionary<string, int>();

        foreach (var tab in gameState)
        {
            var sashimiCards = tab.Played
                .Where(c => c.Type == CardType.Sashimi)
                .ToArray();

            scores.Add(tab.PlayerId, (sashimiCards.Length / 3) * 10);
        }

        return scores;
    }
}