using Akka.Actor;
using Akka.TestKit.Xunit2;

using BoardCutter.Games.Twenty48.Actors;

using static BoardCutter.Core.Tests.TestDataSetup;

namespace BoardCutter.Games.Twenty48.Tests;

[Trait("Category", "UnitTests")]
public class GameActorValidations : TestKit
{
    private readonly TimeSpan _noMsgTimeout = TimeSpan.FromMilliseconds(100);


    [Fact]
    public void GameActor_CanStartGame()
    {
        var writerProbe = CreateTestProbe();
        var creatorPlayer = GetTestPlayer("creator");

        var gameId = "TestGameId";

        var gameActorProps = Props.Create(() =>
            new GameActor(creatorPlayer, writerProbe, new PredictableTilePlacer(),
                gameId));

        var gameActor = Sys.ActorOf(gameActorProps, "gameActor");

        gameActor.Tell(new GameActorMessages.CreateGameRequest(creatorPlayer));


        var msg = ExpectMsg<GameActorMessages.GameCreated>();

        Assert.NotNull(msg);
        Assert.Equivalent(
            new PublicVisible(gameId, 0, []),
            msg.GameData);

        writerProbe.ExpectNoMsg(_noMsgTimeout);
        ExpectNoMsg(_noMsgTimeout);
    }
}