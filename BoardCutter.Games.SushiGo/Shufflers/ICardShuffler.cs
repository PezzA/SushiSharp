namespace BoardCutter.Games.SushiGo.Shufflers;

/// <summary>
/// CardShuffler will randomise a list of cards.
/// </summary>
public interface ICardShuffler
{
    public List<Card> Shuffle(List<Card> cards);
}