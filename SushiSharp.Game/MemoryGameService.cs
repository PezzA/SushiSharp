using SushiSharp.Cards.Decks;
using SushiSharp.Cards.Shufflers;

namespace SushiSharp.Game;

public class MemoryGameService(ICardShuffler cardShuffler) : IGameService
{
    
    private readonly Dictionary<string, GameState?> _gameList = new();

    public async Task<GameState?> StartGame(string gameId)
    {
        if (!_gameList.TryGetValue(gameId, out GameState? gameState))
        {
            return null;
        }

        if (gameState == null) return null;

        gameState = await RunGame(gameState.Id);
        
        return gameState;
    }

    public Task<GameState?> GetGameByPlayer(Player player)
    {
        var game = _gameList
            .Where(g => g.Value != null && g.Value.Players.Any(p => p.Name == player.Name))
            .Select(g => g.Value)
            .SingleOrDefault();
        
        return Task.FromResult(game);
    }

    public Task<GameState> CreateNewGame(Player player)
    {
        var game = new GameState(player);

        _gameList.Add(game.Id, game);

        return Task.FromResult(game);
    }

    public Task<GameState?> GetGame(string gameId)
    {
        if (!_gameList.TryGetValue(gameId, out GameState? value))
        {
            return Task.FromResult<GameState?>(null);
        }
        return Task.FromResult(value);
    }

    public Task<GameState?> JoinGame(string gameId, Player player)
    {
        if (!_gameList.TryGetValue(gameId, out GameState? gameState))
        {
            return Task.FromResult<GameState?>(null);
        }
        
        if(gameState == null)
        {
            return Task.FromResult<GameState?>(null);
        }

        if (gameState.Players.Count >= gameState.Parameters.MaxPlayers)
        {
            return Task.FromResult<GameState?>(null);
        }

        gameState.Players.Add(player);
        
        return Task.FromResult<GameState?>(gameState);
    }

    public Task LeaveCurrentGame(Player player, string gameId)
    {
        if (!_gameList.TryGetValue(gameId, out GameState? value))
        {
            return Task.CompletedTask;
        }

        if (value?.Players[0].Name != player.Name)
        {
            value?.Players.Remove(player);
        }
        else
        {
            RemoveGame(gameId);
        }
        
        return Task.CompletedTask;
    }

    public Task RemoveGame(string gameId)
    {
        _gameList.Remove(gameId);
        
        return Task.CompletedTask;
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
        if (_gameList.Count == 0) return Task.FromResult<List<GameState>>([]);
        
        return Task.FromResult<List<GameState>>(_gameList.Select(g => g.Value).ToList()!);
    }

    public Task<GameState?> RunGame(string gameId)
    {
        if (!_gameList.TryGetValue(gameId, out GameState? gameState))
        {
            return Task.FromResult<GameState?>(null);
        }

        if (gameState == null) return Task.FromResult<GameState?>(null);

        gameState.Deck = cardShuffler.Shuffle(SushiGoClassic.GetDeck());
        gameState.Status = GameStatus.Running;

        return Task.FromResult<GameState?>(gameState);
    }
}