using SushiSharp.Cards;

namespace SushiSharp.Game;

public interface IGameService
{
    Task<GameState> CreateNewGame(Player player);

    Task<GameState?> JoinGame(Guid gameGuid, Player player);

    Task<bool> LeaveCurrentGame(Player player);

    Task<GameState?> SetGameParameters(Guid gameId, GameParameters parameters);

    Task<List<GameState>> GetGames();
}