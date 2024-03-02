namespace SushiSharp.Cards;

public record Tableau(string PlayerId, List<Card> Hand, List<Card> Played, List<Card> Side);