using BoardCutter.Games.Twenty48.Actors;

namespace BoardCutter.Games.Twenty48.Tests;

[Trait("Category", "UnitTests")]
public class ShuntTests
{
    [Theory]
    [InlineData(new[] { 2, 2, 2, 2 }, new[] { 0, 0, 4, 4 }, 8)]
    [InlineData(new[] { 2, 2, 2, 0 }, new[] { 0, 0, 2, 4 }, 4)]
    [InlineData(new[] { 0, 2, 0, 2 }, new[] { 0, 0, 0, 4 }, 4)]
    [InlineData(new[] { 0, 4, 2, 2 }, new[] { 0, 0, 4, 4 }, 4)]
    [InlineData(new[] { 2, 0, 0, 2 }, new[] { 0, 0, 0, 4 }, 4)]
    [InlineData(new[] { 2, 2, 0, 0 }, new[] { 0, 0, 0, 4 }, 4)]
    [InlineData(new[] { 0, 0, 0, 0 }, new[] { 0, 0, 0, 0 }, 0)]
    [InlineData(new[] { 2, 2, 0, 2 }, new[] { 0, 0, 2, 4 }, 4)]
    [InlineData(new[] { 4, 2, 0, 2 }, new[] { 0, 0, 4, 4 }, 4)]
    [InlineData(new[] { 2, 0, 0, 0 }, new[] { 0, 0, 0, 2 }, 0)]
    [InlineData(new[] { 2, 4, 8, 0 }, new[] { 0, 2, 4, 8 }, 0)]
    [InlineData(new[] { 2, 4, 8, 16 }, new[] { 2, 4, 8, 16 }, 0)]
    public void Shunt_Succeeds(int[] input, int[] expectedArray, int expectedIncrement)
    {
        (int[] actualArray, int actualIncrement) = GameActor.Shunt(input);

        Assert.Equal(expectedArray, actualArray);
        Assert.Equal(expectedIncrement, actualIncrement);
    }

    public static IEnumerable<object[]> GetShuntLeftData()
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
                },
                new int[][] 
                {
                    [4, 2, 4, 0],
                    [8, 0, 0, 0],
                    [2, 8, 0, 0],
                    [2, 4, 8, 0]
                }, 12
            }
            // @formatter:on
        };
    }

    [Theory]
    [MemberData(nameof(GetShuntLeftData))]
    public void ShuntLeft_Succeeds(int[][] inputGrid, int[][] expectedGrid, int expectedScoreIncrement)
    {
        (int[][] actualGrid, int actualScoreIncrement) = GameActor.ShuntLeft(inputGrid);

        Assert.Equal(expectedGrid, actualGrid);
        Assert.Equal(expectedScoreIncrement, actualScoreIncrement);
    }

    public static IEnumerable<object[]> GetShuntRightData()
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
                },
                new int[][] 
                {
                    [0, 2, 4, 4],
                    [0, 0, 0, 8],
                    [0, 0, 2, 8],
                    [0, 2, 4, 8]
                }, 12
            }
            // @formatter:on
        };
    }

    [Theory]
    [MemberData(nameof(GetShuntRightData))]
    public void ShuntRight_Succeeds(int[][] inputGrid, int[][] expectedGrid, int expectedScoreIncrement)
    {
        (int[][] actualGrid, int actualScoreIncrement) = GameActor.ShuntRight(inputGrid);

        Assert.Equal(expectedGrid, actualGrid);
        Assert.Equal(expectedScoreIncrement, actualScoreIncrement);
    }

    public static IEnumerable<object[]> GetShuntDownData()
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
                },
                new int[][]
                {
                    [0, 0, 2, 0], 
                    [0, 0, 4, 0], 
                    [0, 0, 2, 8], 
                    [4, 2, 4, 16]
                }, 28
            }
            // @formater:on
        };
    }

    [Theory]
    [MemberData(nameof(GetShuntDownData))]
    public void ShuntDown_Succeeds(int[][] inputGrid, int[][] expectedGrid, int expectedScoreIncrement)
    {
        (int[][] actualGrid, int actualScoreIncrement) = GameActor.ShuntDown(inputGrid);

        Assert.Equal(expectedGrid, actualGrid);
        Assert.Equal(expectedScoreIncrement, actualScoreIncrement);
    }

    public static IEnumerable<object[]> GetShuntUpData()
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
                },
                new int[][]
                {
                    [4, 2, 2, 8], 
                    [0, 0, 4, 16], 
                    [0, 0, 2, 0], 
                    [0, 0, 4, 0]
                }, 28
            }
            // @formater:on
        };
    }
    
    [Theory]
    [MemberData(nameof(GetShuntUpData))]
    public void ShuntUp_Succeeds(int[][] inputGrid, int[][] expectedGrid, int expectedScoreIncrement)
    {
        (int[][] actualGrid, int actualScoreIncrement) = GameActor.ShuntUp(inputGrid);

        Assert.Equal(expectedGrid, actualGrid);
        Assert.Equal(expectedScoreIncrement, actualScoreIncrement);
    }
}