namespace SushiSharp.Cards.Shufflers;

public class RiggedCardShuffler(List<Card> riggedDeck) : ICardShuffler
{
    public List<Card> Shuffle(List<Card> cards)
    {
        return riggedDeck;
    }
}