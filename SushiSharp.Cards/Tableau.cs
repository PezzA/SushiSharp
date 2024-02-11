namespace SushiSharp.Cards;

public record Tableau(string PlayerId, IList<Card> Hand, IList<Card> Played, IList<Card> Side);