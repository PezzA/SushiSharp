namespace SushiSharp.Cards;

public class RiggedCardShuffler(IList<Card> riggedDeck) : ICardShuffler
{
    public IList<Card> Shuffle(IList<Card> cards)
    {
        return riggedDeck;
    }
}