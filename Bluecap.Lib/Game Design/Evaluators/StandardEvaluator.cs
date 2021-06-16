using Bluecap.Lib.Game_Design.Agents;
using Bluecap.Lib.Game_Design.Interfaces;
using Bluecap.Lib.Game_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluecap.Lib.Game_Design.Evaluators
{
    public class StandardEvaluator : IEvaluator<BaseGame>
    {
        public float TimeAllottedPerTurn;
        public int numberOfGamesToTest;

        public int randomRandomMatches;
        public int greedyRandomMatches;
        public int greedySkilledMatches;
        public int skilledMirrorMatches;

        public string bestRulesCode;
        public bool gameTestingFinished;

        public float totalTimeSpent;
        public float estimatedTotalTime;

        public float estimatedTimeLeft;

        public StandardEvaluator(float TimeAllottedPerTurn = 1f, int numberOfGamesToTest = 10, int randomRandomMatches = 20, int greedyRandomMatches = 10, int greedySkilledMatches = 10, int skilledMirrorMatches = 10)
        {
            this.TimeAllottedPerTurn = TimeAllottedPerTurn;
            this.numberOfGamesToTest = numberOfGamesToTest;
            this.randomRandomMatches = randomRandomMatches;
            this.greedyRandomMatches = greedyRandomMatches;
            this.greedySkilledMatches = greedySkilledMatches;
            this.skilledMirrorMatches = skilledMirrorMatches;
        }

        public IEnumerable<BaseGame> Evaluate(IEnumerable<BaseGame> population)
        {
            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 8
            };
            Parallel.ForEach(population, options, (s) =>
            {
                ScoreGameParallelFor(s);
            });
            return population;
        }

        private void PlayGameParallelFor(BaseGame game, BaseAgent player1, BaseAgent player2, int turnLimit = 100)
        {
            int turn = 0;
            //? Always reset the game before playing.
            game.ResetState();

            //var time = Stopwatch.StartNew();

            // game.PrintBoard();
            while (turn < turnLimit)
            {
                if (!player1.TakeTurn(game, TimeAllottedPerTurn))
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

                if (!player2.TakeTurn(game, TimeAllottedPerTurn))
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
                // Debug.Log("Game tied: turn limit exceeded.");
            }
            else
            {
                switch (game.endStatus)
                {
                    case 1:
                        // Debug.Log("Player 1 wins (in "+turn+" turns)");
                        break;
                    case 2:
                        // Debug.Log("Player 2 wins (in "+turn+" turns)");
                        break;
                    case 3:
                        // Debug.Log("Game tied (in "+turn+" turns)");
                        break;
                }
            }

            //yield return 0;
        }

        public void ScoreGameParallelFor(BaseGame game)
        {
            //? Reset
            float playerBiasScore = 0f;
            float greedIsGoodScore = 0f;
            float skillIsBetterScore = 0f;
            float drawsAreBadScore = 0f;
            float highSkillBalanceScore = 0f;
            //string scoresSoFar = "First Play Bias: " + UIManager.Instance.ToScore(playerBiasScore) + "\n";
            //yield return UIManager.Instance.SetCurrentRulesText(game.GameToString());
            //yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar);

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
                PlayGameParallelFor(game, randomAgent1, randomAgent2);
                if (game.endStatus == 1) firstWon++;
                if (game.endStatus == 2) secondWon++;
                //? Yield after each playout - we could yield more frequently, this is OK though.
                //yield return 0;
            }

            playerBiasScore = 1 - (Math.Abs(firstWon - secondWon) / randomRandomMatches);

            //scoresSoFar = "First Play Bias: " + UIManager.Instance.ToScore(playerBiasScore) + "\n";
            //yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar);

            //? We could also add in a measure of 'decisiveness' - i.e. games shouldn't end in draws.
            //? However for random agents this might happen just because they aren't very good.

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
                PlayGameParallelFor(game, randomAgent, greedyAgent);
                if (game.endStatus == 1 + (i % 2))
                {
                    randomAgentWon++;
                }
                //yield return 0;
            }

            greedIsGoodScore = 1 - ((float)randomAgentWon / greedyRandomMatches);


            //scoresSoFar += "Simple Beats Random: " + UIManager.Instance.ToScore(greedIsGoodScore) + "\n";
            //yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar);
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
                PlayGameParallelFor(game, skilledAgent, greedyAgent);
                if (game.endStatus == 1 + (i % 2))
                {
                    mctsAgentWon++;
                }
                //yield return 0;
            }

            skillIsBetterScore = (float)mctsAgentWon / greedySkilledMatches;

            //scoresSoFar += "Clever Beats Simple: " + UIManager.Instance.ToScore(skillIsBetterScore) + "\n";
            //yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar);

            //? Finally, MCTS vs MCTS. If we wanted more depth, we could do two version of this, 
            //? one with MCTS agents that are given different amounts of computation, to really 
            //? test to see if more thinking time = better play. However, here we're just going to
            //? test a good old fashioned mirror matchup. For two good equal players, we want
            //? a) not too much imbalance in favour of either player and b) not too many draws.
            int drawnGames = 0;
            int firstPlayerWon = 0; int secondPlayerWon = 0;
            MCTSAgent skilledAgent1 = new MCTSAgent(1);
            MCTSAgent skilledAgent2 = new MCTSAgent(2);
            for (int i = 0; i < skilledMirrorMatches; i++)
            {
                //yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar +
                //                           "Avoid Draws: Playing (" + i + "/" + skilledMirrorMatches + ")\n" +
                //                           "High Skill Mirror Matchup: Playing (" + i + "/" + skilledMirrorMatches + ")\n");
                ////NOTE MJ: Playing the games could be coroutines, so they don't block UI. res could be an out parameter.
                //yield return PlayGame(game, skilledAgent1, skilledAgent2);
                PlayGameParallelFor(game, skilledAgent1, skilledAgent2);
                if (game.endStatus == 1) firstPlayerWon++;
                if (game.endStatus == 2) secondPlayerWon++;
                if (game.endStatus == 3 || game.endStatus == 0) drawnGames++;
                //yield return 0;
            }

            drawsAreBadScore = 1 - ((float)drawnGames / skilledMirrorMatches);
            highSkillBalanceScore = Math.Abs(firstPlayerWon - secondPlayerWon) / skilledMirrorMatches;

            //yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar + "Avoid Draws: " + UIManager.Instance.ToScore(drawsAreBadScore) + "\n" +
            //"High Skill Mirror Matchup: " + UIManager.Instance.ToScore(highSkillBalanceScore) + "\n");

            //? Now we can add up the scores and return them. If we wanted we could balance them so
            //? some scores are more important than others, or we could partition them into "must-haves"
            //? and "nice-to-haves". I discuss this in the tutorial video.

            // Debug.Log("Random vs. Random: "+playerBiasScore);
            // Debug.Log("Greedy vs. Random: "+greedIsGoodScore);
            // Debug.Log("MCTS vs. Greedy: "+skillIsBetterScore);
            // Debug.Log("MCTS vs. MCTS (draws): "+drawsAreBadScore);
            // Debug.Log("MCTS vs. MCTS (win balance): "+highSkillBalanceScore);

            game.evaluatedScore = (playerBiasScore + greedIsGoodScore + skillIsBetterScore + drawsAreBadScore + highSkillBalanceScore) / 5f;
        }

        public void ScoreGamesParallel(List<BaseGame> games)
        {
            Parallel.ForEach(games, s =>
            {
                ScoreGameParallelFor(s);
            });
        }
    }
}
