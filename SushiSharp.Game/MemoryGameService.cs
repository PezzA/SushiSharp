using SushiSharp.Cards;

namespace SushiSharp.Game;

public class MemoryGameService : IGameService
{
    private readonly Dictionary<string, GameState?> _gameList = new();

    public Task<GameState> CreateNewGame(Player player)
    {
        var game = new GameState(player);

        _gameList.Add(game.Id, game);

        return Task.FromResult(game);
    }

    public Task<GameState?> JoinGame(string gameId, Player player)
    {
        if (!_gameList.TryGetValue(gameId, out GameState? value))
        {
            return Task.FromResult<GameState>(null);
        }
        
        if(value == null)
        {
            return Task.FromResult<GameState>(null);
        }

        value.Players.Add(player);
        
        return Task.FromResult(value);
    }

    public Task<bool> LeaveCurrentGame(Player player)
    {
        throw new NotImplementedException();
    }

    public Task<GameState?> SetGameParameters(string gameId, GameParameters parameters)
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
        return Task.FromResult(_gameList.Select(g => g.Value).ToList());
    }
}