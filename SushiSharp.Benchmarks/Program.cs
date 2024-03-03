using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using SushiSharp.Cards;
using SushiSharp.Cards.Scoring;

namespace SushiSharp.Benchmarks
{
    [MemoryDiagnoser]
    [MinColumn, Q1Column, Q3Column, MaxColumn]
    public class MakiBench
    {
        private readonly List<Tableau> _benchTab;
        private readonly MakiRollScorer _scorer = new();

        private static Tableau CreatePlayedTableau(string playerId, List<Card> played)
        {
            return new Tableau(
                playerId,
                new List<Card>(),
                played,
                new List<Card>()
            );
        }

        public MakiBench()
        {
            // Player 1 has 2 maki rolls
            var player1Tableau = CreatePlayedTableau("P1",
            [
                new Card(1, [CardSymbol.MakiRoll, CardSymbol.MakiRoll],
                    CardType.MakiRolls)
            ]);

            // Player 2 has 3 maki rolls
            var player2Tableau = CreatePlayedTableau("P2",
            [
                new Card(1, [CardSymbol.MakiRoll, CardSymbol.MakiRoll, CardSymbol.MakiRoll],
                    CardType.MakiRolls)
            ]);

            _benchTab = new List<Tableau> { player1Tableau, player2Tableau };
        }

        [Benchmark]
        public Dictionary<string, int> BasicScore() => _scorer.Score(_benchTab);
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<MakiBench>();
        }
    }
}