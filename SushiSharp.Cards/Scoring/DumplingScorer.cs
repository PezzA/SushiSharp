namespace SushiSharp.Cards.Scoring;

public class DumplingScorer : IScorer
{
    public Dictionary<string, int> Score(IList<Tableau> gameState)
    {
        var scores = new Dictionary<string, int>();

        foreach (var tab in gameState)
        {
            var dumplings = tab.Played
                .Where(c => c.Type == CardType.Dumpling)
                .ToArray();

            scores.Add(tab.Player.Id,
                dumplings.Length switch
                {
                    0 => 0,
                    1 => 1,
                    2 => 3,
                    3 => 6,
                    4 => 10,
                    _ => 15
                });
        }

        return scores;
    }
}