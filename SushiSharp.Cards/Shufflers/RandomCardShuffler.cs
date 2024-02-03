namespace SushiSharp.Cards.Shufflers;

public class RandomCardShuffler : ICardShuffler
{
    public IList<Card> Shuffle(IList<Card> cards)
    {
        return cards
            .ToDictionary(k => Guid.NewGuid(), card => card)
            .OrderBy(k => k.Key)
            .Select(kv => kv.Value)
            .ToArray();
    }
}