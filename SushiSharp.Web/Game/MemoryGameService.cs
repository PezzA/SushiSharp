using SushiSharp.Cards;

namespace SushiSharp.Web.Game;

public class MemoryGameService : IGameService
{
    public Task<GameState> CreateNewGame(Player player)
    {
        throw new NotImplementedException();
    }

    public Task<GameState> JoinGame(Guid gameGuid, Player player)
    {
        throw new NotImplementedException();
    }

    public Task<bool> LeaveCurrentGame(Player player)
    {
        throw new NotImplementedException();
    }

    public Task<GameState> SetGameParameters(GameParameters parameters)
    {
        throw new NotImplementedException();
    }
}