namespace SushiSharp.Cards.Scoring;

public class MakiRollScorer : IScorer
{
    public CardType GetCardType { get => CardType.MakiRolls; }
    
    public int Score(IList<Card> tableau, IList<IList<Card>> otherPlayers)
    {
        var myRolls = tableau.Where(c => c.Type == GetCardType).Count();

        var otherRolls = otherPlayers
            .Select(p => p.Where(c => c.Type == GetCardType).Count())
            .Order()
            .ToArray();

        if (myRolls >= otherRolls[0])
        {
            return 6;
        }
        
        if (myRolls >= otherRolls[1])
        {
            return otherPlayers.Count > 5 ? 4 : 3;
        }

        if (otherPlayers.Count < 6) return 0;

        if (otherPlayers.Count > 5 && myRolls >= otherRolls[2])
        {
            return otherPlayers.Count > 5 ? 2 : 0;
        }

        return 0;
    }
}