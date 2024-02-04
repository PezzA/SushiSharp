using SushiSharp.Cards;

namespace SushiSharp.Game;

public class MemoryGameService : IGameService
{
    private readonly Dictionary<Guid, GameState?> _gameList = new();
    public Task<GameState> CreateNewGame(Player player)
    {
        var game = new GameState(player);
        
        _gameList.Add(game.GameGuid, game);

        return Task.FromResult(game);
    }

    public Task<GameState?> JoinGame(Guid gameGuid, Player player)
    {
        throw new NotImplementedException();
    }

    public Task<bool> LeaveCurrentGame(Player player)
    {
        throw new NotImplementedException();
    }

    public Task<GameState?> SetGameParameters(Guid gameId, GameParameters parameters)
    {
        if (!_gameList.TryGetValue(gameId, out GameState? gameState))
        {
            return Task.FromResult<GameState?>(null);
        }

        gameState!.Parameters = parameters;

        return Task.FromResult(gameState)!;
    }

    public Task<List<GameState>> GetGames()
    {
        return Task.FromResult(_gameList.Select( g=> g.Value).ToList());
    }
}