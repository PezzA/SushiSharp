namespace SushiSharp.Cards;

public record Tableau(Player Player, IList<Card> Hand, IList<Card> Played, IList<Card> Side);