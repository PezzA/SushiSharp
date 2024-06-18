namespace BoardCutter.Core.Players;

public interface IPlayerService
{
    Task<Player> AddOrUpdatePlayer(string userName, string connectionId, bool shouldExist);

    Task<Player?> GetPlayerByConnectionId(string id);
    
    Task<Player?> GetPlayerByUser(string user);
}