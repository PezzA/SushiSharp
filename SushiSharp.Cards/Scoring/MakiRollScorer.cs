namespace SushiSharp.Cards.Scoring;

public class MakiRollScorer : IScorer
{
    private int MakiRollSymbolCount(IList<Card> cards)
    {
        return cards.Where(c => c.Type == CardType.MakiRolls).Sum(s => s.Symbols.Length);
    }

    public Dictionary<string, int> Score(IList<Tableau> gameState)
    {
        if (gameState == null || gameState.Count < 2)
            throw new InvalidDataException("Not Enough tableau's to score maki rolls");
        
        var rollCounts = gameState
            .Select(tab => 
                MakiRollSymbolCount(tab.Played))
            .Distinct()
            .OrderByDescending( x=> x)
            .ToArray();

        var scores = new Dictionary<string, int>();

        var isSixToEightPlayers = gameState.Count is > 5 and < 9;

        foreach (var tab in gameState)
        { 
            var makiRollCount = MakiRollSymbolCount(tab.Played);

            if (makiRollCount == 0)
            {
                scores.Add(tab.Player.Id, 0);
                continue;
            }

            var score = 0;
            
            // Top points for most rolls (tied)
            if (makiRollCount == rollCounts[0])
            {
                score += 6;
            }

            // Second place Tied, with player count modifier
            if (makiRollCount == rollCounts[1] && rollCounts.Length > 1)
            {
                score += isSixToEightPlayers ? 4 : 3;
            }

            // If 6 to eight players, score for third place if there are more than 2 distinct results
            if (rollCounts.Length > 2 &&  makiRollCount == rollCounts[2] &&  isSixToEightPlayers)
            {
                score += 2;
            }
            
            scores.Add(tab.Player.Id, score);
        }

        return scores;
    }
}