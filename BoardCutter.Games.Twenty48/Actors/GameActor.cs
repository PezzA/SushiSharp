using System.Text.Json.Nodes;

using Akka.Actor;

using BoardCutter.Core.Actors.HubWriter;
using BoardCutter.Core.Players;
using BoardCutter.Games.SushiGo;

namespace BoardCutter.Games.Twenty48.Actors;

public class ViewerVisibleData
{
    public int Score { get; set; }
    public int[,]? Grid { get; set; }
}

public class GameActor : ReceiveActor
{
    private readonly Player _owner;
    private readonly string _gameId;
    private int _gridSize = 4;
    private int[,] _grid = { { } };
    private int _score = 0;
    private readonly IActorRef _hubWriterActor;

    public GameActor(Player owner, IActorRef hubWriterActor, string? gameId = null)
    {
        _owner = owner;
        _hubWriterActor = hubWriterActor;
        _gameId = gameId ?? Guid.NewGuid().ToString();

        Receive<GameActorMessages.SetupGameRequest>(SetupRequest);
        Receive<GameActorMessages.CreateGameRequest>(CreateGameRequest);
        Receive<GameActorMessages.StartGameRequest>(StartGameRequest);
        Receive<GameActorMessages.MoveRequest>(MoveRequest);
    }

    private void MoveRequest(GameActorMessages.MoveRequest message)
    {
        if (message.Player.Id != _owner.Id)
        {
            // TODO - only game owner can make a move. 
            return;
        }


        (_grid, int scoreIncrement) = message.Direction switch
        {
            Direction.Up => ProcessUp(_grid),
            Direction.Down => ProcessDown(_grid),
            Direction.Left => ProcessLeft(_grid),
            Direction.Right => ProcessRight(_grid),
            _ => throw new ArgumentOutOfRangeException(nameof(message))
        };

        _score += scoreIncrement;
        
        _hubWriterActor.Tell(new HubWriterActorMessages.WriteGroupObject(_gameId,
            ServerMessages.SetViewerVisibleData,
            new ViewerVisibleData() { Score = _score, Grid = _grid }));
    }

    private static (int[,], int, bool) ProcessUpdate(int[,] grid, int x1, int y1, int x2, int y2)
    {
        // exit if source is zero
        if (grid[y1, x1] == 0) return (grid, 0, false);

        int tmpX = x2;
        int tmpY = y2;

        while (grid[tmpY, tmpX] == 0)
        {
            grid[y2, x2] += grid[y1, x1];
            grid[y1, x1] = 0;
        }

        // if target is zero, shunt and set source to zero
        if (grid[y2, x2] == 0)
        {
            grid[y2, x2] += grid[y1, x1];
            grid[y1, x1] = 0;
        }

        // if they are different do nothing
        if (grid[y1, x1] != grid[y2, x2]) return (grid, 0, false);

        // Merge!
        grid[y2, x2] += grid[y1, x1];
        grid[y1, x1] = 0;
        return (grid, grid[y2, x2], true);
    }

    public static (int[,], int) ProcessRight(int[,] grid)
    {
        var gridSize = grid.GetLength(0);
        var scoreIncrement = 0;

        var didUpdate = true;

        while (didUpdate)
        {
            didUpdate = false;
            for (int x = gridSize - 2; x >= 0; x--)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    (grid, int localIncrement, bool updated) = ProcessUpdate(grid, x, y, x + 1, y);

                    scoreIncrement += localIncrement;

                    if (updated)
                    {
                        didUpdate = true;
                    }
                }
            }
        }

        return (grid, scoreIncrement);
    }

    public static (int[,], int) ProcessLeft(int[,] grid)
    {
        var gridSize = grid.GetLength(0);
        var scoreIncrement = 0;

        var didUpdate = true;

        while (didUpdate)
        {
            didUpdate = false;

            for (int x = 1; x <= gridSize - 1; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    (grid, int localIncrement, bool updated) = ProcessUpdate(grid, x, y, x - 1, y);

                    scoreIncrement += localIncrement;

                    if (updated)
                    {
                        didUpdate = true;
                    }
                }
            }
        }

        return (grid, scoreIncrement);
    }

    public static (int[,], int) ProcessUp(int[,] grid)
    {
        var gridSize = grid.GetLength(0);
        var scoreIncrement = 0;

        var didUpdate = true;

        while (didUpdate)
        {
            didUpdate = false;

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 1; y <= gridSize - 1; y++)
                {
                    (grid, int localIncrement, bool updated) = ProcessUpdate(grid, x, y, x, y - 1);

                    scoreIncrement += localIncrement;

                    if (updated)
                    {
                        didUpdate = true;
                    }
                }
            }
        }

        return (grid, scoreIncrement);
    }

    public static (int[,], int) ProcessDown(int[,] grid)
    {
        var gridSize = grid.GetLength(0);
        var scoreIncrement = 0;

        var didUpdate = true;

        while (didUpdate)
        {
            didUpdate = false;

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = gridSize - 2; y >= 0; y--)
                {
                    (grid, int localIncrement, bool updated) = ProcessUpdate(grid, x, y, x, y + 1);

                    scoreIncrement += localIncrement;

                    if (updated)
                    {
                        didUpdate = true;
                    }
                }
            }
        }

        return (grid, scoreIncrement);
    }

    private void StartGameRequest(GameActorMessages.StartGameRequest message)
    {
        _grid = new int[_gridSize, _gridSize];

        var rand = new Random();
        var cellCount = _gridSize * 2;

        var rand1 = rand.Next(cellCount);
        var rand2 = rand.Next(cellCount);

        while (rand1 == rand2)
        {
            rand2 = rand.Next(cellCount);
        }

        (int y1, int x1) = GetXy(rand1, _gridSize);
        (int y2, int x2) = GetXy(rand2, _gridSize);

        _grid[y1, x1] = 2;
        _grid[y2, x2] = 2;

        _score = 0;
    }


    private static (int, int) GetXy(int value, int gridSize)
    {
        return (value / gridSize, value % gridSize);
    }

    private void CreateGameRequest(GameActorMessages.CreateGameRequest message)
    {
    }

    private void SetupRequest(GameActorMessages.SetupGameRequest message)
    {
        if (message.Player.Id != _owner.Id)
        {
            // TODO - only the game owner can change 
            return;
        }

        _gridSize = message.GridSize;
    }
}