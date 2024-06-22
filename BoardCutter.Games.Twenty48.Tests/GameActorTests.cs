using Akka.Actor;
using Akka.TestKit.Xunit2;

using BoardCutter.Core.Actors.HubWriter;
using BoardCutter.Games.Twenty48.Actors;

using static BoardCutter.Core.Tests.TestDataSetup;

namespace BoardCutter.Games.Twenty48.Tests;

[Trait("Category", "UnitTests")]
public class GameActorValidations : TestKit
{
    private readonly TimeSpan _noMsgTimeout = TimeSpan.FromMilliseconds(100);


    [Fact]
    public async void GameActor_CanStartGame()
    {
        var writerProbe = CreateTestProbe();
        var creatorPlayer = GetTestPlayer("creator");

        var gameId = "TestGameId";

        var gameActorProps = Props.Create(() =>
            new GameActor(creatorPlayer, writerProbe, new PredictableTilePlacer(),
                gameId));

        var gameActor = Sys.ActorOf(gameActorProps, "gameActor");

        var response =
            await gameActor.Ask(
                new GameActorMessages.CreateGameRequest(creatorPlayer)) as GameActorMessages.GameCreated;

        Assert.NotNull(response);
        Assert.Equal(gameId, response.GameData.GameId);
        Assert.Equal(0, response.GameData.Score);
        Assert.Equal([], response.GameData.Grid);

        gameActor.Tell(new GameActorMessages.StartGameRequest(creatorPlayer));


        var msg = writerProbe.ExpectMsg<HubWriterActorMessages.WriteClientObject>();

        Assert.IsType<PublicVisible>(msg.Payload);

        var unwrappedMsg = msg.Payload as PublicVisible;

        Assert.NotNull(unwrappedMsg);

        Assert.Equal(gameId, unwrappedMsg.GameId);
        Assert.Equal(0, unwrappedMsg.Score);
        Assert.Equal(new int[][] { [2, 2, 0, 0], [0, 0, 0, 0], [0, 0, 0, 0], [0, 0, 0, 0] }, unwrappedMsg.Grid);
        
        gameActor.Tell(new GameActorMessages.MoveRequest(creatorPlayer, Direction.Right));
        
        var msg2 = writerProbe.ExpectMsg<HubWriterActorMessages.WriteClientObject>();

        Assert.IsType<PublicVisible>(msg2.Payload);

        var unwrappedMsg2 = msg2.Payload as PublicVisible;

        Assert.NotNull(unwrappedMsg2);

        Assert.Equal(gameId, unwrappedMsg2.GameId);
        Assert.Equal(4, unwrappedMsg2.Score);
        Assert.Equal(new int[][] { [2, 0, 0, 4], [0, 0, 0, 0], [0, 0, 0, 0], [0, 0, 0, 0] }, unwrappedMsg2.Grid);
        await writerProbe.ExpectNoMsgAsync(_noMsgTimeout);
        await ExpectNoMsgAsync(_noMsgTimeout);
        
        gameActor.Tell(new GameActorMessages.MoveRequest(creatorPlayer, Direction.Down));
        
        var msg3 = writerProbe.ExpectMsg<HubWriterActorMessages.WriteClientObject>();

        Assert.IsType<PublicVisible>(msg3.Payload);

        var unwrappedMsg3 = msg3.Payload as PublicVisible;

        Assert.NotNull(unwrappedMsg3);

        Assert.Equal(gameId, unwrappedMsg3.GameId);
        Assert.Equal(4, unwrappedMsg3.Score);
        Assert.Equal(new int[][] { [2, 0, 0, 0], [0, 0, 0, 0], [0, 0, 0, 0], [2, 0, 0, 4] }, unwrappedMsg3.Grid);
        
        
        await writerProbe.ExpectNoMsgAsync(_noMsgTimeout);
        await ExpectNoMsgAsync(_noMsgTimeout);
    }
}