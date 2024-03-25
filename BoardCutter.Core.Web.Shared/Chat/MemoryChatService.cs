namespace BoardCutter.Core.Web.Shared.Chat;

public class MemoryChatService : IChatService
{
    private readonly Dictionary<string, List<ChatMessage>> _messageStore = [];
    
    public Task<bool> Add(string key, ChatMessage message)
    {
        if (_messageStore.TryGetValue(key, out List<ChatMessage>? value))
        {
            value.Add(message);
            return Task.FromResult(true);
        }

        _messageStore[key] = [message];
        return Task.FromResult(true);
    }
    
    public Task<List<ChatMessage>> GetMessages(string key)
    {
        return Task.FromResult(_messageStore.ContainsKey(key) ? _messageStore[key] : []);
    }
}