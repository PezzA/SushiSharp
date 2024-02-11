namespace SushiSharp.Cards.Scoring;

public class EmptyScorer : IScorer
{
    public Dictionary<string, int> Score(IList<Tableau> gameState)
        => gameState.ToDictionary(tab => tab.PlayerId, _ => 0);
}
