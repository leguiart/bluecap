using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Assets.Script.Game_Design;
using Unity.Jobs;
using System.Threading.Tasks;
using Unity.Collections;
using Bluecap.Lib.Game_Model;
using Bluecap.Lib.Game_Design.Enums;
using Bluecap.Lib.Game_Design.Agents;

namespace Assets.Script.Game_Design 
{
    public class GameEvaluation : MonoBehaviour
    {

        [Header("Allotted turn taking time in seconds", order = 1)]
        [Space(-10, order = 2)]
        [Header("Longer time leads to better evaluations for Greedy and MCTS", order = 3)]
        public float TimeAllottedPerTurn = 1f;
        [Header("Pool Size")]
        public int numberOfGamesToTest = 10;

        [Header("Testing Composition")]
        public int randomRandomMatches = 20;
        public int greedyRandomMatches = 10;
        public int greedySkilledMatches = 10;
        public int skilledMirrorMatches = 10;

        //[Header("Scene Interface")]
        //public bool interfaceEnabled = false;
        //public TMPro.TextMeshProUGUI bestRulesText;
        //public TMPro.TextMeshProUGUI bestScoresText;
        //public TMPro.TextMeshProUGUI bestOverallScoreText;
        //public TMPro.TextMeshProUGUI currentRulesText;
        //public TMPro.TextMeshProUGUI currentScoresText;
        //public TMPro.TextMeshProUGUI progressBarText;
        //public TMPro.TextMeshProUGUI timeRemainingText;
        //public UnityEngine.UI.Image progressBar;

        [Header("Best Rules")]
        [TextArea]
        public string bestRulesCode;
        public bool gameTestingFinished;

        [Header("Evaluation Time")]
        public float totalTimeSpent;
        public float estimatedTotalTime;

        public float estimatedTimeLeft;


        //? We save the last scores in each category so we can use them, if necessary, in the interface.
        float playerBiasScore = 0;
        float greedIsGoodScore = 0;
        float skillIsBetterScore = 0;
        float drawsAreBadScore = 0;
        float highSkillBalanceScore = 0;

        void Start()
        {
            return;
            Debug.Log("---");
            BaseGame g = new BaseGame(4, 4);
            g.Genotype[2] = new InARowCondition(Direction.LINE, 3);
            g.Genotype[3] = new InARowCondition(Direction.LINE, 2);
            MCTSAgent p = new MCTSAgent(1);
            MCTSAgent q = new MCTSAgent(2);

            g.PrintBoard();
            for (int i = 0; i < 5; i++)
            {
                p.TakeTurn(g);
                g.PrintBoard();
                if (g.endStatus > 0)
                {
                    break;
                }
                q.TakeTurn(g);
                g.PrintBoard();
                if (g.endStatus > 0)
                {
                    break;
                }
            }
            Debug.Log(g.endStatus);
            Debug.Log("---");
        }

        public void PlayRandomGreedy(BaseGame game)
        {
            RandomAgent player1 = new RandomAgent(1);
            GreedyAgent player2 = new GreedyAgent(2);
            PlayGame(game, player1, player2);
        }

        public void PlaySkilledMirrorMatch(BaseGame game)
        {
            MCTSAgent player1 = new MCTSAgent(1);
            MCTSAgent player2 = new MCTSAgent(2);
            PlayGame(game, player1, player2);
        }

        public IEnumerator PlayGame(BaseGame game, BaseAgent player1, BaseAgent player2, int turnLimit = 100)
        {
            int turn = 0;
            //? Always reset the game before playing.
            game.ResetState();

            var time = Stopwatch.StartNew();

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
                if (time.ElapsedMilliseconds > 16)
                {
                    time.Restart();
                    yield return 0;
                }

                if (!player2.TakeTurn(game, TimeAllottedPerTurn))
                    break;
                if (game.endStatus > 0)
                    break;
                // game.PrintBoard();
                turn++;

                //Take a break when we reach 16ms (approx. 1 frame)
                if (time.ElapsedMilliseconds > 16)
                {
                    time.Restart();
                    yield return 0;
                }
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

            yield return 0;
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

            playerBiasScore = 1 - (Mathf.Abs(firstWon - secondWon) / randomRandomMatches);

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
            highSkillBalanceScore = Mathf.Abs(firstPlayerWon - secondPlayerWon) / skilledMirrorMatches;

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

            game.evaluatedScore = (playerBiasScore + greedIsGoodScore + 2.0f * skillIsBetterScore + 1.5f * drawsAreBadScore + 3.0f * highSkillBalanceScore) / 5f;
        }

        public void ScoreGamesParallel(List<BaseGame> games)
        {
            Parallel.ForEach(games, s =>
            {
                ScoreGameParallelFor(s);
            });
        }

        public IEnumerator ScoreGame(BaseGame game)
        {
            //? Reset
            playerBiasScore = 0f;
            greedIsGoodScore = 0f;
            skillIsBetterScore = 0f;
            drawsAreBadScore = 0f;
            highSkillBalanceScore = 0f;
            string scoresSoFar = "First Play Bias: " + UIManager.Instance.ToScore(playerBiasScore) + "\n";
            yield return UIManager.Instance.SetCurrentRulesText(game.GameToString());
            yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar);

            //? Random vs. Random: These games can go either way, the only thing we're interested
            //? is if there's a clear bias towards playing first or second. This is a good indicator.
            //? Score is therefore proportional to the amount one agent won over the other.
            RandomAgent randomAgent1 = new RandomAgent(1);
            RandomAgent randomAgent2 = new RandomAgent(2);
            int firstWon = 0; int secondWon = 0;
            for (int i = 0; i < randomRandomMatches; i++)
            {
                yield return UIManager.Instance.SetCurrentScoresText("First Play Bias: Playing (" + i + "/" + randomRandomMatches + ")\n");
                //NOTE MJ: Playing the games could be coroutines, so they don't block UI.
                //res is redundant game.endStatus already has info.
                yield return PlayGame(game, randomAgent1, randomAgent2);
                if (game.endStatus == 1) firstWon++;
                if (game.endStatus == 2) secondWon++;
                //? Yield after each playout - we could yield more frequently, this is OK though.
                //yield return 0;
            }

            playerBiasScore = 1 - (Mathf.Abs(firstWon - secondWon) / randomRandomMatches);

            scoresSoFar = "First Play Bias: " + UIManager.Instance.ToScore(playerBiasScore) + "\n";
            yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar);

            //? We could also add in a measure of 'decisiveness' - i.e. games shouldn't end in draws.
            //? However for random agents this might happen just because they aren't very good.

            //? Random vs. Greedy: Greedy may not always win the game, but we expect it to
            //? win more than random. Score is proportion to the number of games greedy won or tied.
            int randomAgentWon = 0;
            for (int i = 0; i < greedyRandomMatches; i++)
            {
                yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar + "Simple Beats Random: Playing (" + i + "/" + greedyRandomMatches + ")\n");
                //? Small detail: note that we swap who plays first, to compensate
                //? for first-player advantage
                RandomAgent randomAgent = new RandomAgent(1 + (i % 2));
                GreedyAgent greedyAgent = new GreedyAgent(2 - (i % 2));

                //NOTE MJ: Playing the games could be coroutines, so they don't block UI. res could be an out parameter.
                yield return PlayGame(game, randomAgent, greedyAgent);
                if (game.endStatus == 1 + (i % 2))
                {
                    randomAgentWon++;
                }
                yield return 0;
            }

            greedIsGoodScore = 1 - ((float)randomAgentWon / greedyRandomMatches);


            scoresSoFar += "Simple Beats Random: " + UIManager.Instance.ToScore(greedIsGoodScore) + "\n";
            yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar);
            //? Greedy vs. MCTS: We know that greedy players will avoid causing their own loss, and
            //? win if given the opportunity, but MCTS players can look ahead and plan. As a result,
            //? a more strategic game should be won by MCTS agents. Score is proportion of games MCTS
            //? agent won. Note that we might need to give the MCTS agent more computational resources
            //? for some games to ensure it is performing better.
            int mctsAgentWon = 0;

            for (int i = 0; i < greedySkilledMatches; i++)
            {
                yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar + "Clever Beats Simple: Playing (" + i + "/" + greedySkilledMatches + ")\n");

                MCTSAgent skilledAgent = new MCTSAgent(1 + (i % 2));
                GreedyAgent greedyAgent = new GreedyAgent(2 - (i % 2));

                //NOTE MJ: Playing the games could be coroutines, so they don't block UI. res could be an out parameter.
                yield return PlayGame(game, skilledAgent, greedyAgent);
                if (game.endStatus == 1 + (i % 2))
                {
                    mctsAgentWon++;
                }
                yield return 0;
            }

            skillIsBetterScore = (float)mctsAgentWon / greedySkilledMatches;

            scoresSoFar += "Clever Beats Simple: " + UIManager.Instance.ToScore(skillIsBetterScore) + "\n";
            yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar);

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
                yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar +
                                           "Avoid Draws: Playing (" + i + "/" + skilledMirrorMatches + ")\n" +
                                           "High Skill Mirror Matchup: Playing (" + i + "/" + skilledMirrorMatches + ")\n");
                //NOTE MJ: Playing the games could be coroutines, so they don't block UI. res could be an out parameter.
                yield return PlayGame(game, skilledAgent1, skilledAgent2);
                if (game.endStatus == 1) firstPlayerWon++;
                if (game.endStatus == 2) secondPlayerWon++;
                if (game.endStatus == 3 || game.endStatus == 0) drawnGames++;
                yield return 0;
            }

            drawsAreBadScore = 1 - ((float)drawnGames / skilledMirrorMatches);
            highSkillBalanceScore = Mathf.Abs(firstPlayerWon - secondPlayerWon) / skilledMirrorMatches;

            yield return UIManager.Instance.SetCurrentScoresText(scoresSoFar + "Avoid Draws: " + UIManager.Instance.ToScore(drawsAreBadScore) + "\n" +
            "High Skill Mirror Matchup: " + UIManager.Instance.ToScore(highSkillBalanceScore) + "\n");

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

        public (float, float, float,  float, float) GetScores()
        {
            return (playerBiasScore, greedIsGoodScore, skillIsBetterScore, drawsAreBadScore, highSkillBalanceScore);
        }

        public static GameEvaluation instance;
        void Awake()
        {
            GameEvaluation.instance = this;
        }

    }

}

