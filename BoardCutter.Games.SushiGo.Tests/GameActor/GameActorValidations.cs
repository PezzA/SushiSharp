using Akka.Actor;
using Akka.TestKit.Xunit2;

using BoardCutter.Core.Actors.HubWriter;
using BoardCutter.Core.Players;
using BoardCutter.Games.SushiGo.Actors.Game;
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

    [Fact(Skip= "Amnesty")]
    public void GameActor_DoesNotExceedMaxPlayers()
    {
        var writerProbe = CreateTestProbe();

        var creatorPlayer = GetTestPlayer("creator");
        var guestOne = GetTestPlayer("guestOne");
        var guestTwo = GetTestPlayer("guestTwo");

        var gameId = "TestGameId";
        var gameActorProps = Props.Create(() =>
            new BoardCutter.Games.SushiGo.Actors.Game.GameActor(new RiggedCardShuffler(new List<Card>()), new Dictionary<CardType, IScorer>(),  writerProbe));

        var gameActor = Sys.ActorOf(gameActorProps, "gameActor");

        gameActor.Tell(new GameActorMessages.CreateGameRequest(creatorPlayer, gameId));

        ExpectMsg<GameActorMessages.GameCreated>();
        writerProbe.ExpectMsg<HubWriterActorMessages.WriteClientObject>();

        // Adding the first guest, should be fine
        gameActor.Tell(new GameActorMessages.JoinGameRequest(guestOne, gameId));

        ExpectMsg<GameActorMessages.GameUpdated>();

        // Adding the second guest should trip the max player validation 
        gameActor.Tell(new GameActorMessages.JoinGameRequest(guestTwo, gameId));

        // after this there should be just a single error message sent back to the 
        // originating client.
        var writeMsg = writerProbe.ExpectMsg<HubWriterActorMessages.WriteClientObject>();

        writerProbe.ExpectNoMsg(_noMsgTimeout);
        ExpectNoMsg(_noMsgTimeout);

        Assert.Contains("guestTwo", writeMsg.Player.ConnectionId);
        Assert.Equal(ServerMessages.ErrorMessage, writeMsg.Message);
        Assert.Contains("Max players", writeMsg.Payload.ToString());
    }
}