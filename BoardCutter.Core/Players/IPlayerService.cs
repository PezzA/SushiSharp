namespace BoardCutter.Core.Players;

public interface IPlayerService
{
    Task<Player> AddPlayer(string userName, string connectionId);

    Task<Player?> GetPlayerById(string id);

    Task<Player?> GetPlayerByConnectionId(string id);

    Task<Player?> GetPlayerByUser(string user);
}