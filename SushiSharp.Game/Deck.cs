using SushiSharp.Cards;
using SushiSharp.Cards.Shufflers;

namespace SushiSharp.Game;

public class Deck
{
    private readonly ICardShuffler _cardShuffler;
    private List<Card> _cards;

    public Deck(ICardShuffler cardShuffler, List<Card> cards)
    {
        _cardShuffler = cardShuffler;
        _cards = cards;
    }


    public void Shuffle()
    {
        _cards = _cardShuffler.Shuffle(_cards.ToList());
    }

    public (List<Card> cards, bool endOfDeck) Draw(int numberToDraw)
    {
        var numberToTake = _cards.Count < numberToDraw
            ? _cards.Count
            : numberToDraw;

        var cards = _cards.TakeLast(numberToTake);
        
        _cards.RemoveRange(_cards.Count - numberToTake, numberToTake);
        
        return (cards.ToList(), _cards.Count == 0);
    }
    
}