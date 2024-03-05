namespace SushiSharp.Cards;

public class Tableau(string playerId, List<Card> hand, List<Card> played, List<Card> side)
{
    public string PlayerId { get; set; } = playerId;
    
    public List<Card> Hand { get; set; } = hand;

    public List<Card> Played { get; set; } = played;

    public List<Card> Side { get; set; } = side;

}