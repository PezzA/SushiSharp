using BoardCutter.Games.SushiGo;

namespace BoardCutter.Games.SushiGo.Web.Setup;

public static class CardExtensions
{
    public static string GetCardCssClass(this Card card)
    {
        return card.Type switch
        {
            CardType.MakiRolls => card.Symbols.Length switch
            {
                1 => "maki-1",
                2 => "maki-2",
                3 => "maki-3",
                _ => string.Empty
            },
            CardType.Nagiri => card.Symbols[0] switch
            {
                CardSymbol.NagiriEgg => "egg-nagiri",
                CardSymbol.NagiriSalmon => "salmon-nagiri",
                CardSymbol.NagiriSquid => "squid-nagiri",
                _ => string.Empty
            },
            _ => card.Type switch
            {
                CardType.Tempura => "tempura",
                CardType.Sashimi => "sashimi",
                CardType.Pudding => "pudding",
                CardType.Wasabi => "wasabi",
                CardType.Chopsticks => "chopsticks",
                CardType.Dumpling => "dumpling",
                _ => string.Empty
            }
        };
    }
}