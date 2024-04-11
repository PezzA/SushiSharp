using Akka.Actor;

using BoardCutter.Games.SushiGo.Players;

namespace BoardCutter.Games.Twenty48.Actors;

public class GameActor : ReceiveActor
{
    private readonly Player _owner;
    private string _gameId;
    private int _gridSize = 4;
    private int[,] _grid = { { } };
    private int _score = 0;

    public GameActor(Player owner, string? gameId = null)
    {
        _owner = owner;
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
    }

    public static (int[,], int) ProcessRight(int[,] grid)
    {
        throw new NotImplementedException();
    }

    private static (int[,], int) ProcessLeft(int[,] grid)
    {
        throw new NotImplementedException();
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
                    // exit if source is zero
                    if (grid[y, x] == 0) continue;

                    // if target is zero, shunt and set source to zero
                    if (grid[y - 1, x] == 0)
                    {
                        didUpdate = true;
                        grid[y - 1, x] += grid[y, x];
                        grid[y, x] = 0;
                        continue;
                    }

                    // if they are different do nothing
                    if (grid[y, x] != grid[y - 1, x]) continue;

                    // Merge!
                    didUpdate = true;
                    grid[y - 1, x] += grid[y, x];
                    grid[y, x] = 0;
                    scoreIncrement += grid[y - 1, x];
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
                    // exit if source is zero
                    if (grid[y, x] == 0) continue;

                    // if target is zero, shunt and set source to zero
                    if (grid[y + 1, x] == 0)
                    {
                        didUpdate = true;
                        grid[y + 1, x] += grid[y, x];
                        grid[y, x] = 0;
                        continue;
                    }

                    // if they are different do nothing
                    if (grid[y, x] != grid[y + 1, x]) continue;

                    // Merge!
                    didUpdate = true;
                    grid[y + 1, x] += grid[y, x];
                    grid[y, x] = 0;
                    scoreIncrement += grid[y + 1, x];
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
        throw new NotImplementedException();
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