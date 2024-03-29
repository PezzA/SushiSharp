using Akka.Actor;
using Akka.TestKit.Xunit2;

using BoardCutter.Core.Actors.HubWriter;
using BoardCutter.Games.SushiGo.Actors.Game;
using BoardCutter.Games.SushiGo.Players;
using BoardCutter.Games.SushiGo.Scoring;
using BoardCutter.Games.SushiGo.Shufflers;

namespace BoardCutter.Games.SushiGo.Tests.GameActor;

[Trait("Category", "UnitTests")]
public class GameActorValidations : TestKit
{

    private readonly TimeSpan _noMsgTimeout = TimeSpan.FromMilliseconds(100);
    
    private Player GetTestPlayer(string postfix)
    {
        return new Player($"connection-{postfix}", $"user-{postfix}", $"id-{postfix}");
    }

    [Fact]
    public void GameActor_DoesNotExceedMaxPlayers()
    {
        var writerProbe = CreateTestProbe();

        var creatorPlayer = GetTestPlayer("creator");
        var guestOne = GetTestPlayer("guestOne");
        var guestTwo = GetTestPlayer("guestTwo");

        var gameId = "TestGameId";
        var gameActorProps = Props.Create(() =>
            new BoardCutter.Games.SushiGo.Actors.Game.GameActor(new RiggedCardShuffler(new List<Card>()), new Dictionary<CardType, IScorer>(),  writerProbe, creatorPlayer, gameId));

        var gameActor = Sys.ActorOf(gameActorProps, "gameActor");

        gameActor.Tell(new GameActorMessages.CreateGameRequest(creatorPlayer));

        ExpectMsg<GameActorMessages.UpdateGameNotification>();
        writerProbe.ExpectMsg<ClientWriterActorMessages.AddToGroup>();
        writerProbe.ExpectMsg<ClientWriterActorMessages.WriteClient>();

        // Adding the first guest, should be fine
        gameActor.Tell(new GameActorMessages.JoinGameRequest(guestOne, gameId));

        ExpectMsg<GameActorMessages.UpdateGameNotification>();
        writerProbe.ExpectMsg<ClientWriterActorMessages.AddToGroup>();
        writerProbe.ExpectMsg<ClientWriterActorMessages.WriteGroup>();

        // Adding the second guest should trip the max player validation 
        gameActor.Tell(new GameActorMessages.JoinGameRequest(guestTwo, gameId));

        // after this there should be just a single error message sent back to the 
        // originating client.
        var writeMsg = writerProbe.ExpectMsg<ClientWriterActorMessages.WriteClient>();

        writerProbe.ExpectNoMsg(_noMsgTimeout);
        ExpectNoMsg(_noMsgTimeout);

        Assert.Contains("guestTwo", writeMsg.ConnectionId);
        Assert.Equal(ServerMessages.ErrorMessage, writeMsg.Message);
        Assert.Contains("Max players", writeMsg.Payload);
    }
}