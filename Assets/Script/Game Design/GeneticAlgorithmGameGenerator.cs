using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Collections;
using System.Threading.Tasks;
using Bluecap.Lib.Game_Model;
using Bluecap.Lib.Extensions;

namespace Assets.Script.Game_Design
{
    class GeneticAlgorithmGameGenerator : BaseGameGenerator
    {

        [Header("GA Settings")]
        public int populationSize = 10;
        public int maxIter = 100;
        public int runs = 1;
        public float pc = 0.8f, pm = 0.01f;

        [Header("Performance Settings")]
        public bool parallelEvaluation = false;

        List<BaseGame> Population, BestGames;
        readonly List<float> bestScoresOfRun = new List<float>();
        readonly List<float> averageScoresOfRun = new List<float>();

        int evaluationsSoFar, totalEvaluations;
        float bestScore;

        BaseGame bestGame;
        //List<GameObject> evaluator;
        private List<BaseGame> Populate()
        {
            //evaluator = new List<GameObject>();
            List<BaseGame> population = new List<BaseGame>();
            for(int i = 0; i < populationSize; i++)
            {
                population.Add(GameGeneration.instance.GenerateRandomGame());
                //evaluator.Add(new GameObject($"evaluator{i}", typeof(GameEvaluation)));
            }
                
            return population;
        }

        protected override IEnumerator StartGenerationProcess()
        {
            for(int run = 1; run <= runs; run++)
            {

                bestScoresOfRun.Clear();
                averageScoresOfRun.Clear();
                evaluationsSoFar = 0;
                bestScore = float.MinValue;
                //Overestimate initial evaluation time as: (100 turns per game) * (max time per turn) * (number of games to test)
                totalEvaluations = populationSize * maxIter;
                estimatedTimeLeft = 100 * GameEvaluation.instance.TimeAllottedPerTurn * totalEvaluations;
                BestGames = new List<BaseGame>();
                bestRulesCodes = new List<string>();
                Population = Populate();
                int its = 1;
                while (its <= maxIter)
                {

                    yield return Select(run, its);
                    Crossover();
                    Mutate();
                    its++;
                }

                logger.Log(kTAG, $"Best scores for run({run}/{runs}): ");
                string bestScores = string.Empty;
                foreach (var score in bestScoresOfRun)
                    bestScores += score.ToString() + ", ";
                bestScores = bestScores.TrimEnd(',',' ');
                logger.Log(kTAG, bestScores);

                logger.Log(kTAG, $"Average scores for run({run}/{runs}): ");
                string averageScores = string.Empty;
                foreach (var avg in averageScoresOfRun)
                    averageScores += avg.ToString() + ", ";
                averageScores = averageScores.TrimEnd(',', ' ');
                logger.Log(kTAG, averageScores);

                yield return UIManager.Instance.SetProgressBarFill(1f);
                yield return UIManager.Instance.SetProgressBarText("Generation Process Complete!");
                logger.Log(kTAG, $"Best game code for ({run}/{runs}):\n{bestGame.GameToString()} \nBest game rules for run ({run}/{runs}): \n{bestGame.GameToString()}");
                logger.Log(kTAG, $"Generation Process Complete for run ({run}/{runs})!");
                gameTestingFinished = true;
                // Debug.Log("Best game score: "+bestScore);
                Debug.Log($"Finished evaluating games for run ({run}/{runs}), best game rules found:\n{bestGame.GameToString()}\nCopy this Code into the Play scene to test it yourself:\n{bestGame.GameToCode()}");
            }

        }

        private void Crossover()
        {
            int nCross = Population.Count / 2;
            System.Random rand = new System.Random();
            for (int i = 0; i < nCross; i++)
            {
                float p = (float)rand.NextDouble();
                if(p <= pc)
                {
                    int cross_point = rand.Next(0, Population[0].Genotype.Count - 1);
                    List<object> son1 = Population[2 * i].Genotype.Select((s, index) =>
                    {
                        if (index > cross_point)
                            s = Population[2 * i + 1].Genotype[index];
                        return s;
                    }).ToList();
                    List<object> son2 = Population[2 * i + 1].Genotype.Select((s, index) =>
                    {
                        if (index > cross_point)
                            s = Population[2 * i].Genotype[index];
                        return s;
                    }).ToList();

                    Population[2 * i].Genotype = son1;
                    Population[2 * i + 1].Genotype = son2;
                }
            }
        }

        private void Mutate()
        {
            System.Random rand = new System.Random();
            for(int i = 0; i < populationSize; i++)
            {
                if ((float)rand.NextDouble() <= pm)
                {
                    int geneToMutate = rand.Next(0, Population[0].Genotype.Count);
                    GameGeneration.instance.Mutate(geneToMutate, Population[i].Genotype);
                }
            }
        }

        private IEnumerator Select(int currentRun, int generation)
        {
            if (parallelEvaluation)
            {
                yield return UIManager.Instance.SetProgressBarText($"Evaluating Game {(evaluationsSoFar)}/{totalEvaluations}\nRun: {currentRun}/{runs}");
                yield return UIManager.Instance.SetProgressBarFill((float)(evaluationsSoFar) / totalEvaluations);
                GameEvaluation.instance.ScoreGamesParallel(Population);
            }
            else
            {
                int g = 1;
                int i = 0;
                foreach (var game in Population)
                {
                    yield return UIManager.Instance.SetProgressBarText($"Evaluating Game {(g + evaluationsSoFar)}/{totalEvaluations}\nRun: {currentRun}/{runs}");
                    yield return UIManager.Instance.SetProgressBarFill((float)(g + evaluationsSoFar) / totalEvaluations);
                    yield return GameEvaluation.instance.ScoreGame(game);
                    //StartCoroutine(evaluator[i].GetComponent<GameEvaluation>().ScoreGame(game));
                    //GameObject.Instantiate()
                    if (game.evaluatedScore > bestScore)
                    {
                        bestGame = game;
                        bestScore = game.evaluatedScore;
                        bestRulesCode = game.GameToCode();
                        float playerBiasScore, greedIsGoodScore, skillIsBetterScore, drawsAreBadScore, highSkillBalanceScore;
                        (playerBiasScore, greedIsGoodScore, skillIsBetterScore, drawsAreBadScore, highSkillBalanceScore) = GameEvaluation.instance.GetScores();
                        yield return UIManager.Instance.SetBestScoresText("First Play Bias: " + ToScore(playerBiasScore) + "\n" +
                            "Simple Beats Random: " + ToScore(greedIsGoodScore) + "\n" +
                            "Clever Beats Simple: " + ToScore(skillIsBetterScore) + "\n" +
                            "Avoid Draws: " + ToScore(drawsAreBadScore) + "\n" +
                            "High Skill Mirror Matchup: " + ToScore(highSkillBalanceScore) + "\n");
                        yield return UIManager.Instance.SetBestOverallScoreText("Overall evaluation score: " + ToScore(bestScore));
                        yield return UIManager.Instance.SetBestRuleText(game.GameToString());

                    }
                    g++;
                    i++;
                }
            }


            evaluationsSoFar += Population.Count;
            bestRulesCodes.Clear();


            Population = Population.OrderBy(s => s.evaluatedScore).ToList();
            if (parallelEvaluation)
            {
                if (Population.Last().evaluatedScore > bestScore)
                {
                    bestGame = Population.Last();
                    bestScore = bestGame.evaluatedScore;
                    bestRulesCode = bestGame.GameToCode();
                    yield return UIManager.Instance.SetBestOverallScoreText("Overall evaluation score: " + ToScore(bestScore));
                    yield return UIManager.Instance.SetBestRuleText(bestGame.GameToString());

                }

            }

            bestScoresOfRun.Add(Population.Last().evaluatedScore);
            averageScoresOfRun.Add(Population.Select(s => s.evaluatedScore).ToList().Average());

            var BestGamesGenotypes = BestGames.Select(s => s.Genotype).ToList();
            for(int i = populationSize - 1; i > -1; i--)
            {
                if (!BestGamesGenotypes.Contains(Population[i].Genotype))
                {
                    BestGames.Add(Population[i]);
                    var code = BestGames.Last().GameToCode();
                    logger.Log(kTAG, $"Best novel game found in generation: {generation}({currentRun}/{runs}):\n{bestGame.GameToString()} \nBest novel game rules found in generation {generation}({currentRun}/{runs}): \n{bestGame.GameToString()}\nWith score: {bestScore}");
                    break;
                }
                    
            }

            BestGames = BestGames.OrderBy(s => s.evaluatedScore).ToList();
            //if (BestGames.Count > populationSize)
            //    BestGames.RemoveAt(0);
            foreach (var bestGame in BestGames)
                bestRulesCodes.Add(bestGame.GameToCode());

            float fitnessSum = Population.Select(s => s.evaluatedScore).Sum();
            float sum = 0;
            var cProbSel = Population.Select(s => sum += s.evaluatedScore / fitnessSum).ToList();
            System.Random rand = new System.Random();
            var probs = new List<float>();
            for (int i = 0; i < Population.Count(); i++)
                probs.Add((float)rand.NextDouble());

            for(int k= 0; k < probs.Count; k++)
            {
                int j = cProbSel.BinarySearch2(probs[k]);
                Population[k] = Population[j];
            }
        }
    }
}
