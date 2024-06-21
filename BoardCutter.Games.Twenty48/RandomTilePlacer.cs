using BoardCutter.Core.Exceptions;

namespace BoardCutter.Games.Twenty48;

/// <summary>
/// Random Tile Placer will place a tile in any available cell, 80% change of a 2, 20% change of a 4.
/// </summary>
public class RandomTilePlacer : ITilePlacer
{
    private readonly Random _rand = new();

    public (int, int, int) PlaceTile(int[][] grid)
    {
        int select = _rand.Next(5);

        int tile = select == 0
            ? 4
            : 2;

        List<(int, int)> candidates = [];

        for (int y = 0; y < grid.Length; y++)
        {
            for (int x = 0; x < grid[y].Length; x++)
            {
                if (grid[y][x] == 0)
                {
                    candidates.Add((x, y));
                }
            }
        }

        if (candidates.Count == 0)
        {
            throw new InvalidGameStateException("Tried to get tile for full board");
        }

        int index = _rand.Next(candidates.Count);

        (int pickedX, int pickedY) = candidates[index];

        return (pickedX, pickedY, tile);
    }
}