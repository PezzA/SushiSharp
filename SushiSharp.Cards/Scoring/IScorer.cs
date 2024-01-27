namespace SushiSharp.Cards.Scoring;

public interface IScorer
{
    public CardType GetCardType { get; }
    public int Score(IList<Card> tableau);
}