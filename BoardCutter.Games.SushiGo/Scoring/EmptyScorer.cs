namespace BoardCutter.Games.SushiGo.Scoring;

public class EmptyScorer : IScorer
{
    public Dictionary<string, int> Score(IList<Tableau> gameState)
        => gameState.ToDictionary(tab => tab.PlayerId, _ => 0);
}
