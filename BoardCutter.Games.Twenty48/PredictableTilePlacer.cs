using BoardCutter.Core.Exceptions;

namespace BoardCutter.Games.Twenty48;

/// <summary>
/// Predictable Tile Placer will always put a 2 in the first available slot reading from left to right, top to bottom
/// </summary>
public class PredictableTilePlacer : ITilePlacer
{
    public (int, int, int) PlaceTile(int[][] grid)
    {
        for (int y = 0; y < grid.Length; y++)
        {
            for (int x = 0; x < grid[y].Length; x++)
            {
                if (grid[y][x] == 0)
                {
                    return (x, y, 2);
                }
            }
        }

        throw new InvalidGameStateException("Tried to get tile for full board");
    }
}