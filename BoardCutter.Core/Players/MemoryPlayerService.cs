namespace BoardCutter.Core.Players;

public class MemoryPlayerService : IPlayerService
{
    private readonly Dictionary<string, Player> _playerList = [];

    public Task<Player> AddOrUpdatePlayer(string userName, string connectionId, bool shouldExist)
    {
        if (_playerList.ContainsKey(userName))
        {
            _playerList[userName].ConnectionId = connectionId;
        }
        else
        {
            _playerList[userName] = new Player(connectionId, userName);

        }

        return Task.FromResult(_playerList[userName]);
    }

    public Task<Player?> GetPlayerByConnectionId(string id)
    {
        var player = _playerList.SingleOrDefault(p => p.Value?.ConnectionId == id).Value;

        return Task.FromResult<Player?>(player);
    }

    public Task<Player?> GetPlayerByUser(string user)
    {
        if (_playerList.ContainsKey(user))
        {
            return Task.FromResult(_playerList[user])!;
        }

        return Task.FromResult<Player?>(null);
    }
}