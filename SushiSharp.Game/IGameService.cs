using SushiSharp.Cards;

namespace SushiSharp.Game;

public interface IGameService
{
    Task<GameState?> StartGame(string gameId);
    
    Task<GameState?> GetGameByPlayer(Player player);
    
    Task<GameState> CreateNewGame(Player player);
    
    Task<GameState?> GetGame(string gameId);
    
    Task<GameState?> JoinGame(string gameId, Player playerId);

    Task LeaveCurrentGame(Player playerId, string gameId);

    Task<List<GameState>> GetGames();
}