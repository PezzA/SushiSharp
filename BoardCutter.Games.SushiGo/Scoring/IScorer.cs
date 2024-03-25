namespace BoardCutter.Games.SushiGo.Scoring;

public interface IScorer
{
    public Dictionary<string, int> Score(IList<Tableau> gameState);
}