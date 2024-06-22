using BoardCutter.Games.Twenty48.Actors;

namespace BoardCutter.Games.Twenty48.Tests;

[Trait("Category", "UnitTests")]
public class GameOverTests
{

    public static IEnumerable<object[]> GetGameOverTestTable()
    {
        return new List<object[]>
        {
            // this is the array of test cases: inputGrid, expectedGrid, expectedScoreIncrement
            // @formatter:off
            new object[]
            {
                new int[][] 
                {
                    [2, 2, 2, 4],
                    [0, 0, 4, 4],
                    [0, 0, 2, 8],
                    [2, 0, 4, 8]
                }, false
            },
            new object[]
            {
                new int[][] 
                {
                    [2, 4, 2, 4],
                    [4, 2, 4, 2],
                    [2, 4, 2, 4],
                    [4, 2, 4, 2]
                }, true
                // @formatter:on
            },
            new object[]
            {
                new int[][] 
                {
                    [2, 2, 2, 4],
                    [4, 2, 4, 2],
                    [2, 4, 2, 4],
                    [4, 2, 4, 2]
                }, false
                // @formatter:on
            },
            new object[]
            {
                new int[][] 
                {
                    [2, 4, 2, 4],
                    [4, 2, 4, 2],
                    [2, 4, 2, 4],
                    [4, 2, 4, 0]
                }, false
                // @formatter:on
            }
        };
    }

    [Theory]
    [MemberData(nameof(GetGameOverTestTable))]
    public void IsGameOver_Succeeds(int[][] inputGrid, bool expectedResult)
    {
        var actualResult = GameActor.IsGameOver(inputGrid);

        Assert.Equal(expectedResult, actualResult);
    }
}