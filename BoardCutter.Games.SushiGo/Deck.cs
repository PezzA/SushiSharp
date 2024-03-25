using BoardCutter.Games.SushiGo.Shufflers;

namespace BoardCutter.Games.SushiGo;

public class Deck(ICardShuffler cardShuffler, List<Card> cards)
{
    public void Shuffle()
    {
        cards = cardShuffler.Shuffle(cards.ToList());
    }

    public (List<Card> cards, bool endOfDeck) Draw(int numberToDraw)
    {
        var numberToTake = cards.Count < numberToDraw
            ? cards.Count
            : numberToDraw;

        var cards1 = cards.TakeLast(numberToTake);
        
        cards.RemoveRange(cards.Count - numberToTake, numberToTake);
        
        return (cards1.ToList(), cards.Count == 0);
    }

    public int CardsRemaining() => cards.Count;
}