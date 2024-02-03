namespace SushiSharp.Cards.Shufflers;

public class RiggedCardShuffler(IList<Card> riggedDeck) : ICardShuffler
{
    public IList<Card> Shuffle(IList<Card> cards)
    {
        return riggedDeck;
    }
}