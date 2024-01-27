namespace SushiSharp.Cards.Scoring;

public class ChopStickScorer : IScorer
{
    public CardType GetCardType { get => CardType.Chopsticks; }
    public int Score(IList<Card> tableau) => 0;
}