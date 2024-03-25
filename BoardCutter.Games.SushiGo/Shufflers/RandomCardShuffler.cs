namespace BoardCutter.Games.SushiGo.Shufflers;

public class RandomCardShuffler : ICardShuffler
{
    public List<Card> Shuffle(List<Card> cards)
    {
        return cards
            .ToDictionary(k => Guid.NewGuid(), card => card)
            .OrderBy(k => k.Key)
            .Select(kv => kv.Value)
            .ToList();
    }
}