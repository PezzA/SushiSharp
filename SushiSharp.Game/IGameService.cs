using SushiSharp.Cards;

namespace SushiSharp.Game;

public interface IGameService
{
    Task<GameState> CreateNewGame(Player player);

    Task<GameState?> JoinGame(string gameId, Player player);

    Task<bool> LeaveCurrentGame(Player player);

    Task<GameState?> SetGameParameters(string gameId, GameParameters parameters);

    Task<List<GameState>> GetGames();
}