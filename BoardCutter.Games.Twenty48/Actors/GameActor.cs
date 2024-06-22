using Akka.Actor;

using BoardCutter.Core;
using BoardCutter.Core.Actors.HubWriter;
using BoardCutter.Core.Players;

using Grid = int[][];

namespace BoardCutter.Games.Twenty48.Actors;

public record PublicVisible(string GameId, int Score, Grid Grid, GameStatus Status);

public class GameActor : ReceiveActor
{
    private readonly Player _owner;
    private readonly string _gameId;
    private int _gridSize = 4;
    private Grid _grid = [];
    private int _score;
    private bool _isGameOver;
    private readonly IActorRef _hubWriterActor;
    private readonly ITilePlacer _tilePlacer;
    private GameStatus _gameStatus = GameStatus.SettingUp;

    public GameActor(Player owner, IActorRef hubWriterActor, ITilePlacer tilePlacer, string? gameId = null)
    {
        _owner = owner;
        _hubWriterActor = hubWriterActor;
        _tilePlacer = tilePlacer;
        _gameId = gameId ?? Guid.NewGuid().ToString();
        _tilePlacer = tilePlacer;

        Receive<GameActorMessages.SetupGameRequest>(SetupRequest);
        Receive<GameActorMessages.CreateGameRequest>(CreateGameRequest);
        Receive<GameActorMessages.StartGameRequest>(StartGameRequest);
        Receive<GameActorMessages.MoveRequest>(MoveRequest);
    }

    private void CreateGameRequest(GameActorMessages.CreateGameRequest message)
    {
        Sender.Tell(new GameActorMessages.GameCreated(GetPublicVisibleData()));
    }

    private void SetupRequest(GameActorMessages.SetupGameRequest message)
    {
        if (message.Player.Id != _owner.Id)
        {
            // TODO - only the game owner can change 
            return;
        }

        _gridSize = message.GridSize;

        BroadCastVisible();
    }

    private void MoveRequest(GameActorMessages.MoveRequest message)
    {
        if (message.Player.Id != _owner.Id)
        {
            // TODO - only game owner can make a move. 
            return;
        }

        if (_isGameOver)
        {
            // TODO - Probably log a warning, but other than that, do nothing
            return;
        }

        (_grid, int scoreIncrement) = message.Direction switch
        {
            Direction.Up => ShuntUp(_grid),
            Direction.Down => ShuntDown(_grid),
            Direction.Left => ShuntLeft(_grid),
            Direction.Right => ShuntRight(_grid),
            _ => throw new ArgumentOutOfRangeException(nameof(message))
        };

        _score += scoreIncrement;

        _grid = PlaceNextTile(_grid);

        _isGameOver = IsGameOver(_grid);

        if (_isGameOver)
        {
            SetGameStatus(GameStatus.Complete);
        }

        BroadCastVisible();
    }

    private void SetGameStatus(GameStatus status)
    {
        _gameStatus = status;
        Sender.Tell(new GameActorMessages.GameCreated(GetPublicVisibleData()));
    }

    public static bool IsGameOver(Grid grid)
    {
        for (int y = 0; y < grid.Length; y++)
        {
            for (int x = 0; x < grid[y].Length; x++)
            {
                var cell = grid[y][x];
                if (cell == 0)
                {
                    return false;
                }

                // Are there any adjacent cells of the same value?

                // up
                if (y > 0 && grid[y - 1][x] == cell)
                {
                    return false;
                }

                // down
                if (y < grid.Length - 1 && grid[y + 1][x] == cell)
                {
                    return false;
                }

                // left
                if (x > 0 && grid[y][x - 1] == cell)
                {
                    return false;
                }

                // right
                if (x < grid[y].Length - 1 && grid[y][x + 1] == cell)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static (int[], int) Shunt(int[] input)
    {
        var retVal = input.ToArray();
        var merged = new bool[input.Length];
        var scoreIncrement = 0;

        // Don't need to move the furthest tile, work from right-to-left
        for (var position = input.Length - 2; position >= 0; position--)
        {
            // walk the position from left-to-right
            for (var walkIndex = position; walkIndex < input.Length - 1; walkIndex++)
            {
                // zero can be skipped
                if (retVal[walkIndex] == 0)
                {
                    break;
                }

                var targetIndex = walkIndex + 1;

                // if further along index is zero, swap and carry on
                if (retVal[targetIndex] == 0)
                {
                    retVal[targetIndex] = retVal[walkIndex];
                    retVal[walkIndex] = 0;
                    continue;
                }

                // if next is different or has been merged stop.
                if (retVal[walkIndex] != retVal[targetIndex] || merged[targetIndex])
                {
                    break;
                }

                // merge
                retVal[targetIndex] *= 2;
                retVal[walkIndex] = 0;
                scoreIncrement += retVal[targetIndex];
                merged[targetIndex] = true;
                break;
            }
        }

        return (retVal, scoreIncrement);
    }

    public static (Grid, int) ShuntRight(Grid grid)
    {
        int increment = 0;

        for (int i = 0; i < grid.Length; i++)
        {
            (int[] shunt, int inc) = Shunt(grid[i]);

            grid[i] = shunt;
            increment += inc;
        }

        return (grid, increment);
    }

    public static (Grid, int) ShuntLeft(Grid grid)
    {
        int increment = 0;

        for (int i = 0; i < grid.Length; i++)
        {
            (int[] shunt, int inc) = Shunt(grid[i].Reverse().ToArray());

            grid[i] = shunt.Reverse().ToArray();
            increment += inc;
        }

        return (grid, increment);
    }


    public static (Grid, int) ShuntUp(Grid grid)
    {
        int increment = 0;

        for (var colIndex = 0; colIndex < grid[0].Length; colIndex++)
        {
            var colArray = GetColumnAsArray(grid, colIndex).Reverse().ToArray();

            (int[] shuntedArray, int scoreIncrement) = Shunt(colArray);

            increment += scoreIncrement;

            grid = MapColumnToGrid(grid, shuntedArray.Reverse().ToArray(), colIndex);
        }

        return (grid, increment);
    }

    public static (Grid, int) ShuntDown(Grid grid)
    {
        int increment = 0;

        for (var colIndex = 0; colIndex < grid[0].Length; colIndex++)
        {
            var colArray = GetColumnAsArray(grid, colIndex);

            (int[] shuntedArray, int scoreIncrement) = Shunt(colArray);

            increment += scoreIncrement;

            grid = MapColumnToGrid(grid, shuntedArray, colIndex);
        }

        return (grid, increment);
    }

    private static int[] GetColumnAsArray(Grid grid, int colIndex)
    {
        int[] retVal = new int[grid.Length];

        for (int i = 0; i < grid.Length; i++)
        {
            retVal[i] = grid[i][colIndex];
        }

        return retVal;
    }

    private static Grid MapColumnToGrid(Grid grid, int[] col, int colIndex)
    {
        for (int i = 0; i < col.Length; i++)
        {
            grid[i][colIndex] = col[i];
        }

        return grid;
    }

    private Grid NewGrid(int size)
    {
        var list = new List<int[]>();

        for (int i = 0; i < size; i++)
        {
            list.Add(new int[size]);
        }

        return list.ToArray();
    }

    private Grid PlaceNextTile(Grid input)
    {
        (int x, int y, int value) = _tilePlacer.PlaceTile(input);
        input[y][x] = value;
        return input;
    }

    private void StartGameRequest(GameActorMessages.StartGameRequest message)
    {
        _grid = NewGrid(_gridSize);

        _grid = PlaceNextTile(_grid);
        _grid = PlaceNextTile(_grid);

        _score = 0;

        SetGameStatus(GameStatus.Running);
        BroadCastVisible();
    }

    private PublicVisible GetPublicVisibleData() => new(_gameId, _score, _grid, _gameStatus);

    private void BroadCastVisible() => _hubWriterActor.Tell(new HubWriterActorMessages.WriteClientObject(_owner,
        Server2048Messages.PublicVisible.ToString(),
        GetPublicVisibleData()));
}