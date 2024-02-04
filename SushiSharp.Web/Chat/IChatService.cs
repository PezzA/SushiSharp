namespace SushiSharp.Web.Chat;

public interface IChatService
{
    public Task<bool> Add(string key, ChatMessage message);

    public Task<List<ChatMessage>> GetMessages(string key);
}