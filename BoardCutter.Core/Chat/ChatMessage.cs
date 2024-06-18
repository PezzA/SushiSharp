namespace BoardCutter.Core.Web.Shared.Chat;

public record ChatMessage(string PlayerName, string PlayerAvatarPath, DateTime TimeStamp, string Message);