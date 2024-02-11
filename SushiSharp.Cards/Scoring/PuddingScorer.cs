namespace SushiSharp.Cards.Scoring;

public class PuddingScorer : IScorer
{
    public Dictionary<string, int> Score(IList<Tableau> gameState)
    {
        var scores = new Dictionary<string, int>();

        var rollCounts = gameState
            .Select(tab => tab.Side.Count(s => s.Type == CardType.Pudding))
            .Distinct()
            .OrderByDescending( x=> x)
            .ToArray();

        foreach (var tab in gameState)
        {
            var puddingsForPlayer = tab.Side.Count(s => s.Type == CardType.Pudding);

            if (puddingsForPlayer == rollCounts[0])
            {
                scores.Add(tab.PlayerId, 6);
                continue;
            }

            if (puddingsForPlayer == rollCounts[^1] && gameState.Count > 2)
            {
                scores.Add(tab.PlayerId, -6);
            }
        }

        return scores;
    }
}