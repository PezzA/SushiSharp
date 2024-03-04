namespace SushiSharp.Cards;

public enum MenuSlot
{
    Rolls,
    Appetiser1,
    Appetiser2,
    Appetiser3,
    Special1,
    Special2,
    Desert
}

public static class CardTypeExtensions
{
    public static IEnumerable<CardType?> GetCardsOfType(MenuSlot slot) => slot switch
    {
        MenuSlot.Rolls => [CardType.MakiRolls],
        MenuSlot.Appetiser1
            or MenuSlot.Appetiser2
            or MenuSlot.Appetiser3 =>
            [CardType.Tempura, CardType.Sashimi, CardType.Dumpling],
        MenuSlot.Special1
            or MenuSlot.Special2 => [CardType.Wasabi, CardType.Chopsticks],
        MenuSlot.Desert => [CardType.Pudding],
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
    };
}