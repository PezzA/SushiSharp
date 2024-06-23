using Akka.Actor;
using Akka.TestKit.Xunit2;

using BoardCutter.Core.Actors;
using BoardCutter.Games.SushiGo.Scoring;
using BoardCutter.Games.SushiGo.Shufflers;
using static BoardCutter.Core.Tests.TestDataSetup;

namespace BoardCutter.Games.SushiGo.Tests.GameActor;

[Trait("Category", "UnitTests")]
public class GameActorValidations : TestKit
{
    private readonly TimeSpan _noMsgTimeout = TimeSpan.FromMilliseconds(100);
    
    [Fact]
    public async void GameActor_DoesNotExceedMaxPlayers()
    {
        var writerProbe = CreateTestProbe();

        var creatorPlayer = GetTestPlayer("creator");
        var guestOne = GetTestPlayer("guestOne");
        var guestTwo = GetTestPlayer("guestTwo");

        var gameId = "TestGameId";
        var gameActorProps = Props.Create(() =>
            new BoardCutter.Games.SushiGo.Actors.Game.GameActor(new RiggedCardShuffler(new List<Card>()), new Dictionary<CardType, IScorer>(),  writerProbe));

        var gameActor = Sys.ActorOf(gameActorProps, "gameActor");

        var resp = await gameActor.Ask(new GameManagerMessages.CreateGameSpecificRequest(creatorPlayer, gameId), _noMsgTimeout) as GameManagerNotifications.GameCreated;

        Assert.NotNull(resp);
        
        writerProbe.ExpectMsg<HubWriterMessages.WriteClientObject>(_noMsgTimeout);

        // Adding the first guest, should be fine
        gameActor.Tell(new GameManagerMessages.JoinGameRequest(guestOne, gameId));

        ExpectMsg<GameManagerNotifications.GameUpdated>();
        // Both clients should get public gate state
        writerProbe.ExpectMsg<HubWriterMessages.WriteClientObject>(_noMsgTimeout);
        writerProbe.ExpectMsg<HubWriterMessages.WriteClientObject>(_noMsgTimeout);

        // Adding the second guest should trip the max player validation 
        gameActor.Tell(new GameManagerMessages.JoinGameRequest(guestTwo, gameId));

        // Should write a message to "guestTwo" that max players has been reached
        var writeMsg = writerProbe.ExpectMsg<HubWriterMessages.WriteClientObject>(_noMsgTimeout);

        Assert.Contains("guestTwo", writeMsg.Player.ConnectionId);
        Assert.Equal(ServerMessages.ErrorMessage, writeMsg.Message);
        Assert.Equal(Resources.ResValidationMaxPlayers, writeMsg.Payload.ToString());
        
        await writerProbe.ExpectNoMsgAsync(_noMsgTimeout);
        await ExpectNoMsgAsync(_noMsgTimeout);
    }
}