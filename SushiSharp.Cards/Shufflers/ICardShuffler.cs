namespace SushiSharp.Cards.Shufflers;

/// <summary>
/// CardShuffler will randomise a list of cards.
/// </summary>
public interface ICardShuffler
{
    public IList<Card> Shuffle(IList<Card> cards);
}