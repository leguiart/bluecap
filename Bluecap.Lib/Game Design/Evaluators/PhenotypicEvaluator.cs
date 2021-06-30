using Bluecap.Lib.Game_Design.Agents;
using Bluecap.Lib.Game_Design.Interfaces;
using Bluecap.Lib.Game_Model;
using CCSystem.Lib.Interfaces;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluecap.Lib.Game_Design.Evaluators
{
    public class PhenotypicEvaluator : IEvaluator<BaseGame>
    {
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
        bool randomRandom, randomGreedy, mctsGreedy, mctsMcts, mctsMctsSkilled, logs;
        float bestScore, bestNovelty, bestGenotype;
        BaseGame bestGame, mostNovelGame, bestGenotypeGame;
        protected GenotypeNoveltyEvaluator noveltyEvaluator;
        readonly List<(string, float, float)> BestDistinctGames;
        readonly List<float> bestPhenotypicScoresofRun = new List<float>(), bestNoveltyScoresOfRun = new List<float>(), bestGenotypicScoresOfRun = new List<float>();
        readonly Dictionary<string, List<float>> averageScoresOfRun = new Dictionary<string, List<float>>() { { "noveltyScore", new List<float>() }, { "evaluatedScore", new List<float>() }, { "genotypeScore", new List<float>() }};
        protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ConcurrentDictionary<string, ConcurrentBag<float>> GenerationScores { get; set; }

        public PhenotypicEvaluator(GenotypeNoveltyEvaluator noveltyEvaluator, float timeAllottedPerTurn = 1f, float timeAllottedPerTurnHighSkilled = 1.5f, int numberOfGamesToTest = 10, int randomRandomMatches = 20, int greedyRandomMatches = 10, int greedySkilledMatches = 10, int skilledMirrorMatches = 10, int skilledNonMirrorMatches = 10, int turnLimit = 100, bool randomRandom = true, bool randomGreedy = true, bool mctsGreedy = true, bool mctsMcts = true, bool mctsMctsSkilled = true, int maxIter = 100, bool logs = false)
        {
            TimeAllottedPerTurn = timeAllottedPerTurn;
            TimeAllottedPerTurnHighSkilled = timeAllottedPerTurnHighSkilled;
            this.numberOfGamesToTest = numberOfGamesToTest;
            this.randomRandomMatches = randomRandomMatches;
            this.greedyRandomMatches = greedyRandomMatches;
            this.greedySkilledMatches = greedySkilledMatches;
            this.skilledMirrorMatches = skilledMirrorMatches;
            this.skilledNonMirrorMatches = skilledNonMirrorMatches;
            this.turnLimit = turnLimit;
            this.maxIter = maxIter;
            this.noveltyEvaluator = noveltyEvaluator;
            this.logs = logs;
            GenerationScores = new ConcurrentDictionary<string, ConcurrentBag<float>>();
            GenerationScores.TryAdd("playerBiasScore", new ConcurrentBag<float>());
            GenerationScores.TryAdd("greedIsGoodScore", new ConcurrentBag<float>());
            GenerationScores.TryAdd("skillIsBetterScore", new ConcurrentBag<float>());
            GenerationScores.TryAdd("drawsAreBadScore", new ConcurrentBag<float>());
            GenerationScores.TryAdd("highSkillBalanceScore", new ConcurrentBag<float>());
            GenerationScores.TryAdd("thinkingPaysOutScore", new ConcurrentBag<float>());
            this.randomRandom = randomRandom;
            this.randomGreedy = randomGreedy;
            this.mctsGreedy = mctsGreedy;
            this.mctsMcts = mctsMcts;
            this.mctsMctsSkilled = mctsMctsSkilled;
            BestDistinctGames = new List<(string, float, float)>();
        }

        public IEnumerable<BaseGame> Evaluate(IEnumerable<BaseGame> population)
        {
            //ParallelOptions options = new ParallelOptions()
            //{
            //    MaxDegreeOfParallelism = 6
            //};
            Parallel.ForEach(population, (s, _, index) =>
            {
                ScoreGameParallelFor(s, index);
            });
            its++;
            return population;
        }

        public IEnumerable<BaseGame> EvaluateAsync(IEnumerable<BaseGame> population)
        {
            //ParallelOptions options = new ParallelOptions()
            //{
            //    MaxDegreeOfParallelism = 6
            //};
            Parallel.ForEach(population, (s, _, index) =>
            {
                var t = Task.Run(async ()=> { 
                    await ScoreGameParallelForAsync(s, index); 
                });
                t.Wait();
            });
            its++;
            if (logs)
            {
                population = population.OrderBy(s => s.noveltyScore).ToList();
                bestNoveltyScoresOfRun.Add(population.Last().noveltyScore);
                if (population.Last().noveltyScore > bestNovelty)
                {
                    mostNovelGame = population.Last();
                    bestNovelty = mostNovelGame.noveltyScore;
                }
                //Order population by goal-orienteed fitness (Novelty search terminology) (phenotype evaluation score (Ventura's terminology))
                population = population.OrderBy(s => s.evaluatedScore).ToList();
                bestPhenotypicScoresofRun.Add(population.Last().evaluatedScore);

                if (population.Last().evaluatedScore > bestScore)
                {
                    bestGame = population.Last();
                    bestScore = bestGame.evaluatedScore;
                }

                //Order population by genotype evaluation score (Ventura's terminology)
                population = population.OrderBy(s => s.genotypeScore).ToList();
                bestGenotypicScoresOfRun.Add(population.Last().genotypeScore);

                if (population.Last().genotypeScore > bestGenotype)
                {
                    bestGenotypeGame = population.Last();
                    bestGenotype = bestGenotypeGame.genotypeScore;
                }

                logger.Info($"Best phenotype score of generation {its}: {bestPhenotypicScoresofRun.Last()}");
                logger.Info($"Best novelty score of generation {its}: {bestNoveltyScoresOfRun.Last()}");
                logger.Info($"Best genotype score of generation {its}: {bestGenotypicScoresOfRun.Last()}");

                averageScoresOfRun["evaluatedScore"].Add(population.Select(s => s.evaluatedScore).ToList().Average());
                averageScoresOfRun["noveltyScore"].Add(population.Select(s => s.noveltyScore).ToList().Average());
                averageScoresOfRun["genotypeScore"].Add(population.Select(s => s.genotypeScore).ToList().Average());
                logger.Info($"Average quality of generation {its}: {averageScoresOfRun["evaluatedScore"].Last()}");
                logger.Info($"Average novelty of generation {its}: {averageScoresOfRun["noveltyScore"].Last()}");
                logger.Info($"Average genotype score of generation {its}: {averageScoresOfRun["genotypeScore"].Last()}");
                var poplist = population.ToList();
                //Get best game so far we hadn't found up until now
                var bestDistinctGamesCodes = BestDistinctGames.Select(s => s.Item1);
                for (int i = poplist.Count - 1; i > -1; i--)
                {
                    if (!bestDistinctGamesCodes.Contains(poplist[i].GameToCode()))
                    {
                        var bGame = poplist[i].GameToCode();
                        var bGameString = poplist[i].GameToString();
                        BestDistinctGames.Add((bGame, poplist[i].noveltyScore, poplist[i].evaluatedScore));

                        logger.Info($"Best distinct game code in generation: {its}:\n{bGame}\n" +
                            $"Best distinct game rules found in generation {its}: \n{bGameString}\n" +
                            $"With score: {poplist[i].evaluatedScore}");
                        break;
                    }

                }
                for (int i = 0; i < poplist.Count; i++)
                    poplist[i].genotypeScore = 0;
                return poplist;
            }
            else return population;

        }

        private async Task PlayGameParallelForAsync(BaseGame game, BaseAgent player1, BaseAgent player2, long index, float timeForPlayer1, float timeForPlayer2, int turnLimit = 100)
        {
            int turn = 0;
            //? Always reset the game before playing.
            game.ResetState();

            //var time = Stopwatch.StartNew();
            await Task.Run(() =>
            {
                // game.PrintBoard();
                while (turn < turnLimit)
                {
                    if (!player1.TakeTurn(game, timeForPlayer1))
                        break;
                    if (game.endStatus > 0)
                        break;
                    // game.PrintBoard();
                    turn++;

                    //Take a break when we reach 16ms (approx. 1 frame)
                    //if (time.ElapsedMilliseconds > 16)
                    //{
                    //    time.Restart();
                    //    yield return 0;
                    //}

                    if (!player2.TakeTurn(game, timeForPlayer2))
                        break;
                    if (game.endStatus > 0)
                        break;
                    // game.PrintBoard();
                    turn++;

                    //Take a break when we reach 16ms (approx. 1 frame)
                    //if (time.ElapsedMilliseconds > 16)
                    //{
                    //    time.Restart();
                    //    yield return 0;
                    //}
                }

                // game.PrintBoard();

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
            });


            //yield return 0;
        }

        public async Task ScoreGameParallelForAsync(BaseGame game, long index)
        {
            //? Reset
            float playerBiasScore = 0f;
            float decisiveness = 0f;
            float greedIsGoodScore = 0f;
            float skillIsBetterScore = 0f;
            float drawsAreBadScore = 0f;
            float highSkillBalanceScore = 0f;
            float thinkingPaysOutScore = 0f;
            int numberMeasurements = 0;
            //string scoresSoFar = "First Play Bias: " + UIManager.Instance.ToScore(playerBiasScore) + "\n";
            //yield return UIManager.Instance.SetCurrentRulesText(game.GameToString());
            //yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar);

            if (randomRandom)
            {
                numberMeasurements+=2;
                //? Random vs. Random: These games can go either way, the only thing we're interested
                //? is if there's a clear bias towards playing first or second. This is a good indicator.
                //? Score is therefore proportional to the amount one agent won over the other.
                RandomAgent randomAgent1 = new RandomAgent(1);
                RandomAgent randomAgent2 = new RandomAgent(2);
                int firstWon = 0; int secondWon = 0, draws = 0;

                for (int i = 0; i < randomRandomMatches; i++)
                {
                    //yield return UIManager.Instance.SetCurrentScoresText("First Play Bias: Playing (" + i + "/" + randomRandomMatches + ")\n");
                    //NOTE MJ: Playing the games could be coroutines, so they don't block UI.
                    //res is redundant game.endStatus already has info.
                    //yield return PlayGame(game, randomAgent1, randomAgent2);
                    PlayGameParallelFor(game, randomAgent1, randomAgent2, index, TimeAllottedPerTurn, TimeAllottedPerTurn, turnLimit);
                    if (game.endStatus == 1) firstWon++;
                    if (game.endStatus == 2) secondWon++;
                    if (game.endStatus == 3) draws++;
                    //? Yield after each playout - we could yield more frequently, this is OK though.
                    //yield return 0;
                }

                playerBiasScore = 1f - (Math.Abs(firstWon - secondWon) / (float)randomRandomMatches);
                decisiveness = 1f - draws / (float)randomRandomMatches;
                Console.WriteLine($"(Game-{index}) Random vs. Random done! (game: {index})");
                //? We could also add in a measure of 'decisiveness' - i.e. games shouldn't end in draws.
                //? However for random agents this might happen just because they aren't very good.
            }


            if (randomGreedy)
            {
                numberMeasurements++;
                //? Random vs. Greedy: Greedy may not always win the game, but we expect it to
                //? win more than random. Score is proportion to the number of games greedy won or tied.
                int randomAgentWon = 0;
                for (int i = 0; i < greedyRandomMatches; i++)
                {
                    //yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar + "Simple Beats Random: Playing (" + i + "/" + greedyRandomMatches + ")\n");
                    //? Small detail: note that we swap who plays first, to compensate
                    //? for first-player advantage
                    RandomAgent randomAgent = new RandomAgent(1 + (i % 2));
                    GreedyAgent greedyAgent = new GreedyAgent(2 - (i % 2));

                    //NOTE MJ: Playing the games could be coroutines, so they don't block UI. res could be an out parameter.
                    //yield return PlayGame(game, randomAgent, greedyAgent);
                    PlayGameParallelFor(game, randomAgent, greedyAgent, index, TimeAllottedPerTurn, TimeAllottedPerTurn, turnLimit);
                    if (game.endStatus == 1 + (i % 2))
                    {
                        randomAgentWon++;
                    }
                    //yield return 0;
                }

                greedIsGoodScore = 1f - ((float)randomAgentWon / greedyRandomMatches);
                Console.WriteLine($"(Game-{index}) Random vs. Greedy done! (game: {index})");
            }


            if (mctsGreedy)
            {
                numberMeasurements++;
                //? Greedy vs. MCTS: We know that greedy players will avoid causing their own loss, and
                //? win if given the opportunity, but MCTS players can look ahead and plan. As a result,
                //? a more strategic game should be won by MCTS agents. Score is proportion of games MCTS
                //? agent won. Note that we might need to give the MCTS agent more computational resources
                //? for some games to ensure it is performing better.
                ConcurrentBag<int> mctsAgentWonConcurrent = new ConcurrentBag<int>();
                ConcurrentStack<BaseGame> baseGames = new ConcurrentStack<BaseGame>();

                Task[] tasks = new Task[greedySkilledMatches];
                for (int i = 0; i < greedySkilledMatches; i++)
                    baseGames.Push(game.Copy());
                for (int i = 0; i < greedySkilledMatches; i++)
                {
                    //yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar + "Clever Beats Simple: Playing (" + i + "/" + greedySkilledMatches + ")\n");



                    //NOTE MJ: Playing the games could be coroutines, so they don't block UI. res could be an out parameter.
                    //yield return PlayGame(game, skilledAgent, greedyAgent);
                    tasks[i] = Task.Run(async () => {
                        MCTSAgent skilledAgent = new MCTSAgent(1 + (i % 2));
                        GreedyAgent greedyAgent = new GreedyAgent(2 - (i % 2));
                        int tries = 0;
                        BaseGame g = null;
                        while (!baseGames.TryPop(out g) && tries < 100)
                            tries++;
                        if (g != null)
                        {
                            await PlayGameParallelForAsync(g, skilledAgent, greedyAgent, index, TimeAllottedPerTurn, TimeAllottedPerTurn, turnLimit);
                            if (g.endStatus == 1 + (i % 2))
                                mctsAgentWonConcurrent.Add(1);
                        }
                    });

                    //yield return 0;
                }
                await Task.WhenAll(tasks);
                skillIsBetterScore = (float)mctsAgentWonConcurrent.Sum() / greedySkilledMatches;
                Console.WriteLine($"(Game-{index}) MCTS vs. Greedy done! (game: {index})");
            }


            //? Finally, MCTS vs MCTS. If we wanted more depth, we could do two version of this, 
            //? one with MCTS agents that are given different amounts of computation, to really 
            //? test to see if more thinking time = better play. However, here we're just going to
            //? test a good old fashioned mirror matchup. For two good equal players, we want
            //? a) not too much imbalance in favour of either player and b) not too many draws.
            if (mctsMcts)
            {
                numberMeasurements ++;
                ConcurrentBag<int> firstPlayerWonConcurrent = new ConcurrentBag<int>();
                ConcurrentBag<int> secondPlayerWonConcurrent = new ConcurrentBag<int>();
                ConcurrentBag<int> drawnGamesConcurrent = new ConcurrentBag<int>();
                ConcurrentStack<BaseGame> baseGames = new ConcurrentStack<BaseGame>();

                Task[] tasks = new Task[skilledMirrorMatches];
                for (int i = 0; i < skilledMirrorMatches; i++)
                    baseGames.Push(game.Copy());
                for (int i = 0; i < skilledMirrorMatches; i++)
                {
                    //yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar +
                    //                           "Avoid Draws: Playing (" + i + "/" + skilledMirrorMatches + ")\n" +
                    //                           "High Skill Mirror Matchup: Playing (" + i + "/" + skilledMirrorMatches + ")\n");
                    ////NOTE MJ: Playing the games could be coroutines, so they don't block UI. res could be an out parameter.
                    //yield return PlayGame(game, skilledAgent1, skilledAgent2);
                    tasks[i] = Task.Run(async () => 
                    {
                        MCTSAgent skilledAgent1 = new MCTSAgent(1);
                        MCTSAgent skilledAgent2 = new MCTSAgent(2);
                        int tries = 0;
                        BaseGame g = null;
                        while (!baseGames.TryPop(out g) && tries < 100)
                            tries++;
                        if(g != null)
                        {
                            await PlayGameParallelForAsync(g, skilledAgent1, skilledAgent2, index, TimeAllottedPerTurnHighSkilled, TimeAllottedPerTurnHighSkilled, turnLimit);
                            if (g.endStatus == 1) firstPlayerWonConcurrent.Add(1);
                            if (g.endStatus == 2) secondPlayerWonConcurrent.Add(1);
                            if (g.endStatus == 3 || g.endStatus == 0) drawnGamesConcurrent.Add(1);
                        }

                    });

                    //yield return 0;
                }
                await Task.WhenAll(tasks);
                float drawnCount = (float)drawnGamesConcurrent.Sum();
                drawsAreBadScore = 1f - (drawnCount / skilledMirrorMatches);
                highSkillBalanceScore = drawsAreBadScore *(1 - Math.Abs((float)firstPlayerWonConcurrent.Sum() - (float)secondPlayerWonConcurrent.Sum()) / (float)skilledMirrorMatches);
                Console.WriteLine($"(Game-{index}) MCTS vs. MCTS done! (game: {index})");
            }


            if (mctsMctsSkilled)
            {
                numberMeasurements++;
                ConcurrentBag<int> firstPlayerWonConcurrent = new ConcurrentBag<int>();
                ConcurrentBag<int> secondPlayerWonConcurrent = new ConcurrentBag<int>();
                ConcurrentStack<BaseGame> baseGames = new ConcurrentStack<BaseGame>();

                Task[] tasks = new Task[skilledMirrorMatches];
                for (int i = 0; i < skilledMirrorMatches; i++)
                    baseGames.Push(game.Copy());
                for (int i = 0; i < skilledNonMirrorMatches; i++)
                {
                    tasks[i] = Task.Run(async () =>
                    {
                        MCTSAgent skilledAgent1 = new MCTSAgent(1);
                        MCTSAgent skilledAgent2 = new MCTSAgent(2);
                        int tries = 0;
                        BaseGame g = null;
                        while (!baseGames.TryPop(out g) && tries < 100)
                            tries++;
                        if (g != null)
                        {
                            await PlayGameParallelForAsync(g, skilledAgent1, skilledAgent2, index, TimeAllottedPerTurnHighSkilled, TimeAllottedPerTurnHighSkilled / 2f, turnLimit);
                            if (g.endStatus == 1) firstPlayerWonConcurrent.Add(1);
                            if (g.endStatus == 2) secondPlayerWonConcurrent.Add(1);
                        }

                    });
                    //yield return 0;
                }
                await Task.WhenAll(tasks);
                thinkingPaysOutScore = 1 - Math.Abs(0.5f * firstPlayerWonConcurrent.Sum() - secondPlayerWonConcurrent.Sum()) / skilledNonMirrorMatches;
                Console.WriteLine($"(Game-{index}) MCTS vs. MCTS low skill done! (game: {index})");
            }

            //yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar + "Avoid Draws: " + UIManager.Instance.ToScore(drawsAreBadScore) + "\n" +
            //"High Skill Mirror Matchup: " + UIManager.Instance.ToScore(highSkillBalanceScore) + "\n");

            //? Now we can add up the scores and return them. If we wanted we could balance them so
            //? some scores are more important than others, or we could partition them into "must-haves"
            //? and "nice-to-haves". I discuss this in the tutorial video.

            if (randomRandom)
            {
                Console.WriteLine($"(Game-{index}) Random vs. Random: " + playerBiasScore);
                GenerationScores["playerBiasScore"].Add(playerBiasScore);
            }

            if (randomGreedy)
            {
                Console.WriteLine($"(Game-{index}) Greedy vs. Random: " + greedIsGoodScore);
                GenerationScores["greedIsGoodScore"].Add(greedIsGoodScore);
            }

            if (mctsGreedy)
            {
                Console.WriteLine($"(Game-{index}) MCTS vs. Greedy: " + skillIsBetterScore);
                GenerationScores["skillIsBetterScore"].Add(skillIsBetterScore);
            }

            if (mctsMcts)
            {
                Console.WriteLine($"(Game-{index}) MCTS vs. MCTS (draws): " + drawsAreBadScore);
                Console.WriteLine($"(Game-{index}) MCTS vs. MCTS (win balance): " + highSkillBalanceScore);
                GenerationScores["drawsAreBadScore"].Add(drawsAreBadScore);
                GenerationScores["highSkillBalanceScore"].Add(highSkillBalanceScore);
            }

            if (mctsMctsSkilled)
            {
                Console.WriteLine($"(Game-{index}) MCTS vs. MCTS less skilled (win balance): " + thinkingPaysOutScore);
                GenerationScores["thinkingPaysOutScore"].Add(highSkillBalanceScore);
            }

            game.evaluatedScore = (playerBiasScore + decisiveness + greedIsGoodScore + skillIsBetterScore + highSkillBalanceScore + thinkingPaysOutScore) / (float)numberMeasurements;
            Console.WriteLine($"Evaluation Score (game: {index}): " + game.evaluatedScore);
        }

        private void PlayGameParallelFor(BaseGame game, BaseAgent player1, BaseAgent player2, long index, float timeForPlayer1, float timeForPlayer2, int turnLimit = 100)
        {
            int turn = 0;
            //? Always reset the game before playing.
            game.ResetState();

            //var time = Stopwatch.StartNew();

            // game.PrintBoard();
            while (turn < turnLimit)
            {
                if (!player1.TakeTurn(game, timeForPlayer1))
                    break;
                if (game.endStatus > 0)
                    break;
                // game.PrintBoard();
                turn++;

                //Take a break when we reach 16ms (approx. 1 frame)
                //if (time.ElapsedMilliseconds > 16)
                //{
                //    time.Restart();
                //    yield return 0;
                //}

                if (!player2.TakeTurn(game, timeForPlayer2))
                    break;
                if (game.endStatus > 0)
                    break;
                // game.PrintBoard();
                turn++;

                //Take a break when we reach 16ms (approx. 1 frame)
                //if (time.ElapsedMilliseconds > 16)
                //{
                //    time.Restart();
                //    yield return 0;
                //}
            }

            // game.PrintBoard();

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

            //yield return 0;
        }

        public void ScoreGameParallelFor(BaseGame game, long index)
        {
            //? Reset
            float playerBiasScore = 0f;
            float greedIsGoodScore = 0f;
            float skillIsBetterScore = 0f;
            float drawsAreBadScore = 0f;
            float highSkillBalanceScore = 0f;
            float thinkingPaysOutScore = 0f;
            int numberMeasurements = 0;
            //string scoresSoFar = "First Play Bias: " + UIManager.Instance.ToScore(playerBiasScore) + "\n";
            //yield return UIManager.Instance.SetCurrentRulesText(game.GameToString());
            //yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar);

            if (randomRandom)
            {
                numberMeasurements++;
                //? Random vs. Random: These games can go either way, the only thing we're interested
                //? is if there's a clear bias towards playing first or second. This is a good indicator.
                //? Score is therefore proportional to the amount one agent won over the other.
                RandomAgent randomAgent1 = new RandomAgent(1);
                RandomAgent randomAgent2 = new RandomAgent(2);
                int firstWon = 0; int secondWon = 0;
                for (int i = 0; i < randomRandomMatches; i++)
                {
                    //yield return UIManager.Instance.SetCurrentScoresText("First Play Bias: Playing (" + i + "/" + randomRandomMatches + ")\n");
                    //NOTE MJ: Playing the games could be coroutines, so they don't block UI.
                    //res is redundant game.endStatus already has info.
                    //yield return PlayGame(game, randomAgent1, randomAgent2);
                    PlayGameParallelFor(game, randomAgent1, randomAgent2, index, TimeAllottedPerTurn, TimeAllottedPerTurn, turnLimit);
                    if (game.endStatus == 1) firstWon++;
                    if (game.endStatus == 2) secondWon++;
                    //? Yield after each playout - we could yield more frequently, this is OK though.
                    //yield return 0;
                }

                playerBiasScore = 1 - (Math.Abs(firstWon - secondWon) / randomRandomMatches);
                Console.WriteLine($"(Game-{index}) Random vs. Random done! (game: {index})");
                //? We could also add in a measure of 'decisiveness' - i.e. games shouldn't end in draws.
                //? However for random agents this might happen just because they aren't very good.
            }


            if (randomGreedy)
            {
                numberMeasurements++;
                //? Random vs. Greedy: Greedy may not always win the game, but we expect it to
                //? win more than random. Score is proportion to the number of games greedy won or tied.
                int randomAgentWon = 0;
                for (int i = 0; i < greedyRandomMatches; i++)
                {
                    //yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar + "Simple Beats Random: Playing (" + i + "/" + greedyRandomMatches + ")\n");
                    //? Small detail: note that we swap who plays first, to compensate
                    //? for first-player advantage
                    RandomAgent randomAgent = new RandomAgent(1 + (i % 2));
                    GreedyAgent greedyAgent = new GreedyAgent(2 - (i % 2));

                    //NOTE MJ: Playing the games could be coroutines, so they don't block UI. res could be an out parameter.
                    //yield return PlayGame(game, randomAgent, greedyAgent);
                    PlayGameParallelFor(game, randomAgent, greedyAgent, index, TimeAllottedPerTurn, TimeAllottedPerTurn, turnLimit);
                    if (game.endStatus == 1 + (i % 2))
                    {
                        randomAgentWon++;
                    }
                    //yield return 0;
                }

                greedIsGoodScore = 1 - ((float)randomAgentWon / greedyRandomMatches);
                Console.WriteLine($"(Game-{index}) Random vs. Greedy done! (game: {index})");
            }


            if (mctsGreedy)
            {
                numberMeasurements++;
                //? Greedy vs. MCTS: We know that greedy players will avoid causing their own loss, and
                //? win if given the opportunity, but MCTS players can look ahead and plan. As a result,
                //? a more strategic game should be won by MCTS agents. Score is proportion of games MCTS
                //? agent won. Note that we might need to give the MCTS agent more computational resources
                //? for some games to ensure it is performing better.
                int mctsAgentWon = 0;

                for (int i = 0; i < greedySkilledMatches; i++)
                {
                    //yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar + "Clever Beats Simple: Playing (" + i + "/" + greedySkilledMatches + ")\n");

                    MCTSAgent skilledAgent = new MCTSAgent(1 + (i % 2));
                    GreedyAgent greedyAgent = new GreedyAgent(2 - (i % 2));

                    //NOTE MJ: Playing the games could be coroutines, so they don't block UI. res could be an out parameter.
                    //yield return PlayGame(game, skilledAgent, greedyAgent);
                    PlayGameParallelFor(game, skilledAgent, greedyAgent, index, TimeAllottedPerTurn, TimeAllottedPerTurn, turnLimit);
                    if (game.endStatus == 1 + (i % 2))
                    {
                        mctsAgentWon++;
                    }
                    //yield return 0;
                }

                skillIsBetterScore = (float)mctsAgentWon / greedySkilledMatches;
                Console.WriteLine($"(Game-{index}) MCTS vs. Greedy done! (game: {index})");
            }

            //? Finally, MCTS vs MCTS. If we wanted more depth, we could do two version of this, 
            //? one with MCTS agents that are given different amounts of computation, to really 
            //? test to see if more thinking time = better play. However, here we're just going to
            //? test a good old fashioned mirror matchup. For two good equal players, we want
            //? a) not too much imbalance in favour of either player and b) not too many draws.
            int drawnGames = 0;
            int firstPlayerWon = 0; int secondPlayerWon = 0;
            MCTSAgent skilledAgent1 = new MCTSAgent(1);
            MCTSAgent skilledAgent2 = new MCTSAgent(2);

            if (mctsMcts)
            {
                numberMeasurements+=2;
                for (int i = 0; i < skilledMirrorMatches; i++)
                {
                    //yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar +
                    //                           "Avoid Draws: Playing (" + i + "/" + skilledMirrorMatches + ")\n" +
                    //                           "High Skill Mirror Matchup: Playing (" + i + "/" + skilledMirrorMatches + ")\n");
                    ////NOTE MJ: Playing the games could be coroutines, so they don't block UI. res could be an out parameter.
                    //yield return PlayGame(game, skilledAgent1, skilledAgent2);
                    PlayGameParallelFor(game, skilledAgent1, skilledAgent2, index, TimeAllottedPerTurnHighSkilled, TimeAllottedPerTurnHighSkilled, turnLimit);
                    if (game.endStatus == 1) firstPlayerWon++;
                    if (game.endStatus == 2) secondPlayerWon++;
                    if (game.endStatus == 3 || game.endStatus == 0) drawnGames++;
                    //yield return 0;
                }

                drawsAreBadScore = 1 - ((float)drawnGames / skilledMirrorMatches);
                highSkillBalanceScore = 1 - Math.Abs((float)firstPlayerWon - (float)secondPlayerWon) / skilledMirrorMatches;
                Console.WriteLine($"(Game-{index}) MCTS vs. MCTS done! (game: {index})");
            }


            if (mctsMctsSkilled)
            {
                numberMeasurements++;
                firstPlayerWon = 0; secondPlayerWon = 0; drawnGames = 0;
                skilledAgent1 = new MCTSAgent(1);
                skilledAgent2 = new MCTSAgent(2);
                for (int i = 0; i < skilledNonMirrorMatches; i++)
                {
                    //yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar +
                    //                           "Avoid Draws: Playing (" + i + "/" + skilledMirrorMatches + ")\n" +
                    //                           "High Skill Mirror Matchup: Playing (" + i + "/" + skilledMirrorMatches + ")\n");
                    ////NOTE MJ: Playing the games could be coroutines, so they don't block UI. res could be an out parameter.
                    //yield return PlayGame(game, skilledAgent1, skilledAgent2);
                    PlayGameParallelFor(game, skilledAgent1, skilledAgent2, index, TimeAllottedPerTurnHighSkilled, TimeAllottedPerTurnHighSkilled / 2f, turnLimit);
                    if (game.endStatus == 1) firstPlayerWon++;
                    if (game.endStatus == 2) secondPlayerWon++;
                    if (game.endStatus == 3 || game.endStatus == 0) drawnGames++;
                    //yield return 0;
                }
                drawsAreBadScore = 1 - ((float)drawnGames / skilledMirrorMatches);
                thinkingPaysOutScore = 1 - Math.Abs(0.5f * firstPlayerWon - secondPlayerWon) / skilledNonMirrorMatches;
                Console.WriteLine($"(Game-{index}) MCTS vs. MCTS low skill done! (game: {index})");
            }

            //yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar + "Avoid Draws: " + UIManager.Instance.ToScore(drawsAreBadScore) + "\n" +
            //"High Skill Mirror Matchup: " + UIManager.Instance.ToScore(highSkillBalanceScore) + "\n");

            //? Now we can add up the scores and return them. If we wanted we could balance them so
            //? some scores are more important than others, or we could partition them into "must-haves"
            //? and "nice-to-haves". I discuss this in the tutorial video.

            if (randomRandom)
            {
                Console.WriteLine($"(Game-{index}) Random vs. Random: " + playerBiasScore);
                GenerationScores["playerBiasScore"].Add(playerBiasScore);
            }

            if (randomGreedy)
            {
                Console.WriteLine($"(Game-{index}) Greedy vs. Random: " + greedIsGoodScore);
                GenerationScores["greedIsGoodScore"].Add(greedIsGoodScore);
            }

            if (mctsGreedy)
            {
                Console.WriteLine($"(Game-{index}) MCTS vs. Greedy: " + skillIsBetterScore);
                GenerationScores["skillIsBetterScore"].Add(skillIsBetterScore);
            }
                
            if (mctsMcts)
            {
                Console.WriteLine($"(Game-{index}) MCTS vs. MCTS (draws): " + drawsAreBadScore);
                Console.WriteLine($"(Game-{index}) MCTS vs. MCTS (win balance): " + highSkillBalanceScore);
                GenerationScores["drawsAreBadScore"].Add(drawsAreBadScore);
                GenerationScores["highSkillBalanceScore"].Add(highSkillBalanceScore);
            }

            if (mctsMctsSkilled)
            {
                Console.WriteLine($"(Game-{index}) MCTS vs. MCTS less skilled (win balance): " + thinkingPaysOutScore);
                GenerationScores["thinkingPaysOutScore"].Add(highSkillBalanceScore);
            }
                
            game.evaluatedScore = (playerBiasScore + greedIsGoodScore + skillIsBetterScore + drawsAreBadScore + highSkillBalanceScore + thinkingPaysOutScore) / (float)numberMeasurements;
            Console.WriteLine($"Evaluation Score (game: {index}): " + game.evaluatedScore);
        }

        public bool StopCriteria(IEnumerable<BaseGame> population)
        {
            bool stopCondition = its >= maxIter;
            if(stopCondition)
                its = 0;
            return stopCondition;
        }

        public IEnumerable<BaseGame> ExtractArtifacts(IEnumerable<BaseGame> population)
        {
            logger.Info($"Best phenotype scores for run: ");
            string bestScores = string.Empty;
            foreach (var score in bestPhenotypicScoresofRun)
                bestScores += score.ToString() + ", ";
            bestScores = bestScores.TrimEnd(',', ' ');
            logger.Info(bestScores);

            logger.Info($"Most novel scores for run: ");
            bestScores = string.Empty;
            foreach (var score in bestNoveltyScoresOfRun)
                bestScores += score.ToString() + ", ";
            bestScores = bestScores.TrimEnd(',', ' ');
            logger.Info(bestScores);

            logger.Info($"Best genotype scores for run: ");
            bestScores = string.Empty;
            foreach (var score in bestGenotypicScoresOfRun)
                bestScores += score.ToString() + ", ";
            bestScores = bestScores.TrimEnd(',', ' ');
            logger.Info(bestScores);

            logger.Info($"Average scores for run: ");
            foreach (var kv in averageScoresOfRun)
            {
                string averageScores = string.Empty;
                foreach (var avg in kv.Value)
                    averageScores += avg.ToString() + ", ";
                averageScores = averageScores.TrimEnd(',', ' ');
                logger.Info($"{kv.Key}:\n{averageScores}");
            }

            logger.Info($"Novelty archive for run: ");
            int j = 0;
            foreach (var elem in noveltyEvaluator.NoveltyArchive)
                logger.Info($"Novelty archive item[{j}]:\n" +
                    $"{elem.Key}\n" +
                    $"Novelty score = {elem.Value.Item1}\n" +
                    $"Fitness score = {elem.Value.Item2}");

            logger.Info($"Fittest distinct games for run: ");
            for (int i = 0; i < BestDistinctGames.Count; i++)
                logger.Info($"Best distinct game item[{i}]:\n" +
                    $"{BestDistinctGames[i].Item1}\n" +
                    $"Novelty score = {BestDistinctGames[i].Item2}\n" +
                    $"Fitness score = {BestDistinctGames[i].Item3}");

            logger.Info($"Best game code for :\n{bestGame.GameToCode()}\n" +
                $"Best game rules for run : \n{bestGame.GameToString()}\n" +
                $"With fitness score: {bestScore}\n" +
                $"Novelty score: {bestGame.noveltyScore}");

            logger.Info($"Most novel game code:\n{mostNovelGame.GameToCode()}\n" +
                $"Most novel game rules for run: \n{mostNovelGame.GameToString()}\n" +
                $"With fitness score {mostNovelGame.evaluatedScore}\n" +
                $"Novelty score: {bestNovelty}");
            logger.Info($"Generation Process Complete for run!");
            return population;
        }
    }
}
