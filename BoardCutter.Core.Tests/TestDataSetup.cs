using BoardCutter.Core.Players;

namespace BoardCutter.Core.Tests;

public static class TestDataSetup
{
    public static Player GetTestPlayer(string postfix)
    {
        return new Player($"connection-{postfix}", $"user-{postfix}", $"id-{postfix}");
    }
}