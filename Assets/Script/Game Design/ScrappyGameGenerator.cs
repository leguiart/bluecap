using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Assets.Script.Game_Design;
using Bluecap.Lib.Game_Model;

namespace Assets.Script.Game_Design 
{
    public class ScrappyGameGenerator : BaseGameGenerator
    {
        BaseGame bestGame;
        /*
        *  Now we can put it all together.
        *
        *  This is the most basic, silly kind of game generator possible. 
        *  It randomly generates games, tests them, and at the end gives you the best one.
        *  It's included here as an example of how to use agents to rank games. Ideally we'd
        *  probably opt for a more 'intelligent' design process, perhaps an evolutionary one
        *  that uses agent ranking as fitness. If I get time I'll add this to the codebase,
        *  but as I'm putting this together I wanted to include a basic example just to be safe.
        * 
        */

        [Header("Pool Size")]
        public int numberOfGamesToTest = 10;



        protected override IEnumerator StartGenerationProcess()
        {
            //Overestimate initial evaluation time as: (100 turns per game) * (max time per turn) * (number of games to test)
            estimatedTimeLeft = 100 * GameEvaluation.instance.TimeAllottedPerTurn * numberOfGamesToTest;

            float bestScore = float.MinValue;


            //var timer = Stopwatch.StartNew();

            for (int g = 0; g < numberOfGamesToTest; g++)
            {
                UIManager.Instance.SetProgressBarText("Evaluating Game " + g + "/" + numberOfGamesToTest);
                UIManager.Instance.SetProgressBarFill((float)g / numberOfGamesToTest);

                BaseGame game = GameGeneration.instance.GenerateRandomGame();

                //NOTE MJ: Don't think you need to StartCoroutine: "yield return ScoreGame(game);" should do.
                yield return GameEvaluation.instance.ScoreGame(game);

                //NOTE: use StopWatch instead of Unity Time, because that is Time since frame start, not actual time.
                //float timeTaken = timer.ElapsedMilliseconds / 1000f;
                //timer.Restart();

                //This is a pretty bad estimate, because some games are a lot harder to evaluate than others.
                //In particular, in search-based approaches (like computational evolution) will find better
                //games as the search goes on, which means your system will get slower as the generation goes 
                //on. That doesn't happen here because our approach is completely random.
                //totalTimeSpent += timeTaken;
                //var averageTimeTaken = totalTimeSpent / (g + 1);
                //estimatedTotalTime = averageTimeTaken * numberOfGamesToTest;
                //estimatedTimeLeft = estimatedTotalTime - totalTimeSpent;

                //var t = TimeSpan.FromSeconds(estimatedTimeLeft);

                //timeRemainingText.text = "Estimated time remaining: " + string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                //    t.Hours,
                //    t.Minutes,
                //    t.Seconds);

                if (game.evaluatedScore > bestScore)
                {
                    bestGame = game;
                    bestScore = game.evaluatedScore;
                    bestRulesCode = game.GameToCode();
                    float playerBiasScore, greedIsGoodScore, skillIsBetterScore, drawsAreBadScore, highSkillBalanceScore;
                    (playerBiasScore, greedIsGoodScore, skillIsBetterScore, drawsAreBadScore, highSkillBalanceScore) = GameEvaluation.instance.GetScores();
                    UIManager.Instance.SetBestScoresText("First Play Bias: " + ToScore(playerBiasScore) + "\n" +
                        "Simple Beats Random: " + ToScore(greedIsGoodScore) + "\n" +
                        "Clever Beats Simple: " + ToScore(skillIsBetterScore) + "\n" +
                        "Avoid Draws: " + ToScore(drawsAreBadScore) + "\n" +
                        "High Skill Mirror Matchup: " + ToScore(highSkillBalanceScore) + "\n");
                        UIManager.Instance.SetBestOverallScoreText("Overall evaluation score: " + ToScore(bestScore));
                    UIManager.Instance.SetBestRuleText(game.GameToString());

                }
            }
            UIManager.Instance.SetProgressBarFill(1f);
            UIManager.Instance.SetProgressBarText("Generation Process Complete!");
            gameTestingFinished = true;
            // Debug.Log("Best game score: "+bestScore);
            Debug.Log("Finished evaluating games, best game rules found:\n" +
                      bestGame.GameToString() +
                      "\n" +
                      "Copy this Code into the Play scene to test it yourself: \n" +
                      bestGame.GameToCode());
        }

    }
}

