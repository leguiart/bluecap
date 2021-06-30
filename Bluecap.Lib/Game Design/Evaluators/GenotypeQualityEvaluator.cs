using Bluecap.Lib.Game_Design.Agents;
using Bluecap.Lib.Game_Model;
using CCSystem.Lib.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluecap.Lib.Game_Design.Evaluators
{
    public class GenotypeQualityEvaluator : IEvaluator<BaseGame>
    {
        public ConcurrentDictionary<string, ConcurrentBag<float>> GenerationScores { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float TimeAllottedPerTurn;
        public float TimeAllottedPerTurnHighSkilled;
        public int numberOfGamesToTest;

        public int randomRandomMatches;
        public int greedyRandomMatches;
        public int greedySkilledMatches;
        public int skilledMirrorMatches;
        public int skilledNonMirrorMatches;

        public string bestRulesCode;
        public bool gameTestingFinished;

        public float totalTimeSpent;
        public float estimatedTotalTime;

        public float estimatedTimeLeft;
        int turnLimit, maxIter, its = 0;
        float genotypeQualityThresh;
        public GenotypeQualityEvaluator(float timeAllottedPerTurn = 1f, int randomRandomMatches = 20, int turnLimit = 100, float genotypeQualityThresh = 0.8f, int maxIter = 100)
        {
            TimeAllottedPerTurn = timeAllottedPerTurn;
            this.randomRandomMatches = randomRandomMatches;
            this.turnLimit = turnLimit;
            this.genotypeQualityThresh = genotypeQualityThresh;
            this.maxIter = maxIter;
        }

        public IEnumerable<BaseGame> Evaluate(IEnumerable<BaseGame> population)
        {
            Parallel.ForEach(population, (s, _, index) =>
            {
                ScoreGameParallelFor(s, index);
            });
            its++;
            return population;
        }
        public void ScoreGameParallelFor(BaseGame game, long index)
        {
            //? Reset
            float playerBiasScore = 0f, decisiveness = 0f, turnsScore = 0f;

            //? Random vs. Random: These games can go either way, the only thing we're interested
            //? is if there's a clear bias towards playing first or second. This is a good indicator.
            //? Score is therefore proportional to the amount one agent won over the other.
            RandomAgent randomAgent1 = new RandomAgent(1);
            RandomAgent randomAgent2 = new RandomAgent(2);
            int firstWon = 0; int secondWon = 0, draws = 0, totalTurns = 0, maxTurns = turnLimit* randomRandomMatches;
            for (int i = 0; i < randomRandomMatches; i++)
            {
                int turns = PlayGameParallelFor(game, randomAgent1, randomAgent2, index, TimeAllottedPerTurn, TimeAllottedPerTurn, turnLimit);
                if (game.endStatus == 1) firstWon++;
                if (game.endStatus == 2) secondWon++;
                if (game.endStatus == 3) draws++;
                totalTurns += turns;
            }

            playerBiasScore = 1f - (Math.Abs(firstWon - secondWon) / (float)randomRandomMatches);
            //? We could also add in a measure of 'decisiveness' - i.e. games shouldn't end in draws.
            //? However for random agents this might happen just because they aren't very good.
            decisiveness = 1f - draws / (float)randomRandomMatches;
            turnsScore = 1f - totalTurns / (float)maxTurns;
            Console.WriteLine($"(Game-{index}) Genotype Random vs. Random done! (game: {index})");

            Console.WriteLine($"(Game-{index}) Random vs. Random: " + playerBiasScore);
                //GenerationScores["playerBiasScore"].Add(playerBiasScore);

            game.qualityScore = (playerBiasScore + decisiveness + turnsScore) / 3f;
            Console.WriteLine($"Genotype Quality Score (game: {index}): " + game.evaluatedScore);
        }

        private int PlayGameParallelFor(BaseGame game, BaseAgent player1, BaseAgent player2, long index, float timeForPlayer1, float timeForPlayer2, int turnLimit = 100)
        {
            int turn = 0;
            game.ResetState();

            while (turn < turnLimit)
            {
                if (!player1.TakeTurn(game, timeForPlayer1))
                    break;
                if (game.endStatus > 0)
                    break;
                turn++;

                if (!player2.TakeTurn(game, timeForPlayer2))
                    break;
                if (game.endStatus > 0)
                    break;
                turn++;

            }

            if (turn >= turnLimit)
            {
                Console.WriteLine($"(Game-{index}) Game tied: turn limit exceeded.");
            }
            else
            {
                switch (game.endStatus)
                {
                    case 1:
                        Console.WriteLine($"(Game-{index}) Player 1 wins (in " + turn + " turns)");
                        break;
                    case 2:
                        Console.WriteLine($"(Game-{index}) Player 2 wins (in " + turn + " turns)");
                        break;
                    case 3:
                        Console.WriteLine($"(Game-{index}) Game tied (in " + turn + " turns)");
                        break;
                }
            }
            return turn;
            //yield return 0;
        }

        public IEnumerable<BaseGame> EvaluateAsync(IEnumerable<BaseGame> population)
        {
            Parallel.ForEach(population, (s, _, index) =>
            {
                ScoreGameParallelFor(s, index);
            });

            return population;
        }

        public IEnumerable<BaseGame> ExtractArtifacts(IEnumerable<BaseGame> population)
        {
            float qAvg = population.Select(s => s.qualityScore).Average();
            return population.Where(s => s.qualityScore >= qAvg);
        }

        public bool StopCriteria(IEnumerable<BaseGame> population)
        {
            return ExtractArtifacts(population).Count() > 0 || its >= maxIter;
        }
    }
}
