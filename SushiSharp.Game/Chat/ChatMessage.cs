namespace SushiSharp.Game.Chat;

public record ChatMessage(Player Player, DateTime TimeStamp, string Message);