namespace BoardCutter.Games.Twenty48;

public interface ITilePlacer
{
    (int, int, int) PlaceTile(int[][] grid);
}