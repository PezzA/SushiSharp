using SushiSharp.Cards;

namespace SushiSharp.Web.Game;

public interface IGameService
{
    Task<GameState> CreateNewGame(Player player);

    Task<GameState> JoinGame(Guid gameGuid, Player player);

    Task<bool> LeaveCurrentGame(Player player);

    Task<GameState> SetGameParameters(GameParameters parameters);
    
    
}