namespace SushiSharp.Game;

public interface IPlayerService
{
    Task<Player> AddPlayer(string userName, string connectionId);

    Task<Player?> GetPlayerById(string id);

    Task<Player?> GetPlayerByConnectionId(string id);

    Task<Player?> GetPlayerByUser(string user);
}

public class MemoryPlayerService : IPlayerService
{
    private readonly Dictionary<string, Player?> _playerList = [];

    public Task<Player> AddPlayer(string userName, string connectionId)
    {
        var player = new Player(connectionId, userName);

        _playerList.Add(player.Id, player);

        return Task.FromResult(player);
    }

    public Task<Player?> GetPlayerById(string id)
    {
        if (!_playerList.TryGetValue(id, out Player? player))
        {
            return Task.FromResult<Player?>(null);
        }

        return Task.FromResult(player);
    }

    public Task<Player?> GetPlayerByConnectionId(string id)
    {
        var player = _playerList.SingleOrDefault(p => p.Value?.ConnectionId == id).Value;

        return Task.FromResult<Player?>(player);
    }

    public Task<Player?> GetPlayerByUser(string user)
    {
        var player = _playerList.SingleOrDefault(p => p.Value?.Name == user).Value;

        return Task.FromResult<Player?>(player);
    }
}