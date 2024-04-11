using BoardCutter.Games.Twenty48.Actors;

namespace BoardCutter.Games.Twenty48.Tests;

[Trait("Category", "UnitTests")]
public class GameTests
{
    [Fact]
    public void MovingDown_Processes_Successfully()
    {
        var grid = new[,] { { 2, 2, 2, 4 }, { 0, 0, 4, 4 }, { 0, 0, 2, 8 }, { 2, 0, 4, 8 } };

        var expected = new[,] { { 0, 0, 2, 0 }, { 0, 0, 4, 0 }, { 0, 0, 2, 8 }, { 4, 2, 4, 16 } };

        (int[,] actual, int scoreIncrement) = GameActor.ProcessDown(grid);

        Assert.Equal(28, scoreIncrement);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void MovingUp_Processes_Successfully()
    {
        var grid = new[,] { { 2, 2, 2, 4 }, { 0, 0, 4, 4 }, { 0, 0, 2, 8 }, { 2, 0, 4, 8 } };

        var expected = new[,] { { 4, 2, 2, 8 }, { 0, 0, 4, 16 }, { 0, 0, 2, 0 }, { 0, 0, 4, 0 } };

        (int[,] actual, int scoreIncrement) = GameActor.ProcessUp(grid);

        Assert.Equal(28, scoreIncrement);
        Assert.Equivalent(expected, actual);
    }
}