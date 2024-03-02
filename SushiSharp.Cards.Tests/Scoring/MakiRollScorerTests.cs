using SushiSharp.Cards.Scoring;

namespace SushiSharp.Cards.Tests.Scoring;

[Trait("Category", "Scoring")]
public class MakiRollScorerTests
{
    private static Tableau CreatePlayedTableau(string playerId, List<Card> played)
    {
        return new Tableau(
            playerId,
            new List<Card>(),
            played,
            new List<Card>()
        );
    }

    [Fact]
    public void TwoPlayers_BothWithMaki_SingleWinner()
    {
        // Player 1 has 2 maki rolls
        var player1Tableau = CreatePlayedTableau("P1",
            new List<Card>()
            {
                new([CardSymbol.MakiRoll, CardSymbol.MakiRoll],
                    CardType.MakiRolls)
            });

        // Player 2 has 3 maki rolls
        var player2Tableau = CreatePlayedTableau("P2",
            new List<Card>()
            {
                new([CardSymbol.MakiRoll, CardSymbol.MakiRoll, CardSymbol.MakiRoll],
                    CardType.MakiRolls)
            });

        var scorer = new MakiRollScorer();

        var makiScores = scorer.Score(new List<Tableau> { player1Tableau, player2Tableau });
        
        Assert.Equal(6, makiScores["P2"]);
        Assert.Equal(3, makiScores["P1"]);
    }
    
    /// <summary>
    /// You have to have a maki roll to be considered for scoring. In the case player 2 gets no points.
    /// </summary>
    [Fact]
    public void TwoPlayers_OneWithMaki_SingleWinner()
    {
        // Player 1 has 2 maki rolls
        var player1Tableau = CreatePlayedTableau("P1",
            new List<Card>()
            {
                new([CardSymbol.MakiRoll, CardSymbol.MakiRoll],
                    CardType.MakiRolls)
            });

        // Player 2 has 3 maki rolls
        var player2Tableau = CreatePlayedTableau("P2",
            new List<Card>()
            {
                new([CardSymbol.Sashimi],
                    CardType.Sashimi)
            });

        var scorer = new MakiRollScorer();

        var makiScores = scorer.Score(new List<Tableau> { player1Tableau, player2Tableau });
        
        Assert.Equal(0, makiScores["P2"]);
        Assert.Equal(6, makiScores["P1"]);
    }
    
    [Fact]
    public void TwoPlayers_BothWithNone_NoPoints()
    {
        // Player 1 has 2 maki rolls
        var player1Tableau = CreatePlayedTableau("P1",
            new List<Card>()
            {
                new([CardSymbol.Tempura],
                    CardType.Tempura)
            });

        // Player 2 has 3 maki rolls
        var player2Tableau = CreatePlayedTableau("P2",
            new List<Card>()
            {
                new([CardSymbol.Sashimi],
                    CardType.Sashimi)
            });

        var scorer = new MakiRollScorer();

        var makiScores = scorer.Score(new List<Tableau> { player1Tableau, player2Tableau });
        
        Assert.Equal(0, makiScores["P2"]);
        Assert.Equal(0, makiScores["P1"]);
    }
}