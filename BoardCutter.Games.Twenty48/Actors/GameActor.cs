﻿using Akka.Actor;

using BoardCutter.Core.Actors.HubWriter;
using BoardCutter.Core.Players;
using BoardCutter.Games.SushiGo;

using Grid = int[][];

namespace BoardCutter.Games.Twenty48.Actors;

public class ViewerVisibleData
{
    public int Score { get; set; }
    public Grid Grid { get; set; } = [];
}

public class GameActor : ReceiveActor
{
    private readonly Player _owner;
    private readonly string _gameId;
    private int _gridSize = 4;
    private Grid _grid = [];
    private int _score;
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
            Direction.Up => ShuntUp(_grid),
            Direction.Down => ShuntDown(_grid),
            Direction.Left => ShuntLeft(_grid),
            Direction.Right => ShuntRight(_grid),
            _ => throw new ArgumentOutOfRangeException(nameof(message))
        };

        _score += scoreIncrement;

        _hubWriterActor.Tell(new HubWriterActorMessages.WriteGroupObject(_gameId,
            ServerMessages.SetViewerVisibleData,
            new ViewerVisibleData() { Score = _score, Grid = _grid }));
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

    private Grid NewGrid(int size)
    {
        var list = new List<int[]>();

        for (int i = 0; i < size; i++)
        {
            list.Add(new int[size]);
        }

        return list.ToArray();
    }

    private void StartGameRequest(GameActorMessages.StartGameRequest message)
    {
        _grid = NewGrid(_gridSize);

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

        _grid[y1][x1] = 2;
        _grid[y2][x2] = 2;

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