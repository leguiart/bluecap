using Bluecap.Lib.Extensions;
using Bluecap.Lib.Game_Design.Interfaces;
using Bluecap.Lib.Game_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluecap.Lib.Game_Design.Generators
{
    public class GeneticAlgorithmGameGenerator : BaseGameGenerator
    {
        public int populationSize;
        public int maxIter;
        public int runs;
        public float pc, pm;

        public bool parallelEvaluation = false;

        List<BaseGame> Population, BestGames;
        readonly List<float> bestScoresOfRun = new List<float>();
        readonly List<float> averageScoresOfRun = new List<float>();

        int evaluationsSoFar, totalEvaluations;
        float bestScore;

        BaseGame bestGame;

        public GeneticAlgorithmGameGenerator(GenerationSettings settings, IEvaluator<BaseGame> gameEvaluator, int populationSize = 10, int maxIter = 100, int runs = 1, float pc = 0.8f, float pm = 0.01f) : base(settings, gameEvaluator) 
        {
            this.populationSize = populationSize;
            this.maxIter = maxIter;
            this.runs = runs;
            this.pc = pc;
            this.pm = pm;
        }

        //List<GameObject> evaluator;
        private List<BaseGame> Populate()
        {
            //evaluator = new List<GameObject>();
            List<BaseGame> population = new List<BaseGame>();
            for (int i = 0; i < populationSize; i++)
            {
                population.Add(GameGenerationUtils.GenerateRandomGame(settings));
                //evaluator.Add(new GameObject($"evaluator{i}", typeof(GameEvaluation)));
            }

            return population;
        }

        public override void StartGenerationProcess()
        {
            logger.Info("Starting game generation run...");
            for (int run = 1; run <= runs; run++)
            {

                bestScoresOfRun.Clear();
                averageScoresOfRun.Clear();
                evaluationsSoFar = 0;
                bestScore = float.MinValue;
                //Overestimate initial evaluation time as: (100 turns per game) * (max time per turn) * (number of games to test)
                totalEvaluations = populationSize * maxIter;
                //estimatedTimeLeft = 100 * GameEvaluation.instance.TimeAllottedPerTurn * totalEvaluations;
                BestGames = new List<BaseGame>();
                bestRulesCodes = new List<string>();
                Population = Populate();
                int its = 1;
                while (its <= maxIter)
                {

                    Select(run, its);
                    Crossover();
                    Mutate();
                    its++;
                }

                logger.Info($"Best scores for run({run}/{runs}): ");
                string bestScores = string.Empty;
                foreach (var score in bestScoresOfRun)
                    bestScores += score.ToString() + ", ";
                bestScores = bestScores.TrimEnd(',', ' ');
                logger.Info(bestScores);

                logger.Info($"Average scores for run({run}/{runs}): ");
                string averageScores = string.Empty;
                foreach (var avg in averageScoresOfRun)
                    averageScores += avg.ToString() + ", ";
                averageScores = averageScores.TrimEnd(',', ' ');
                logger.Info(averageScores);

                logger.Info($"Best game code for ({run}/{runs}):\n{bestGame.GameToString()} \nBest game rules for run ({run}/{runs}): \n{bestGame.GameToString()}");
                logger.Info($"Generation Process Complete for run ({run}/{runs})!");
                gameTestingFinished = true;
                // Debug.Log("Best game score: "+bestScore);
            }

        }

        private void Crossover()
        {
            int nCross = Population.Count / 2;
            System.Random rand = new System.Random();
            for (int i = 0; i < nCross; i++)
            {
                float p = (float)rand.NextDouble();
                if (p <= pc)
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
            for (int i = 0; i < populationSize; i++)
            {
                if ((float)rand.NextDouble() <= pm)
                {
                    int geneToMutate = rand.Next(0, Population[0].Genotype.Count);
                    GameGenerationUtils.Mutate(geneToMutate, Population[i].Genotype, settings);
                }
            }
        }

        private void Select(int currentRun, int generation)
        {
            Population = gameEvaluator.Evaluate(Population).ToList();


            evaluationsSoFar += Population.Count;
            bestRulesCodes.Clear();


            Population = Population.OrderBy(s => s.evaluatedScore).ToList();
            if (Population.Last().evaluatedScore > bestScore)
            {
                bestGame = Population.Last();
                bestScore = bestGame.evaluatedScore;
                bestRulesCode = bestGame.GameToCode();
            }


            bestScoresOfRun.Add(Population.Last().evaluatedScore);
            averageScoresOfRun.Add(Population.Select(s => s.evaluatedScore).ToList().Average());

            var BestGamesGenotypes = BestGames.Select(s => s.Genotype).ToList();
            for (int i = populationSize - 1; i > -1; i--)
            {
                if (!BestGamesGenotypes.Contains(Population[i].Genotype))
                {
                    BestGames.Add(Population[i]);
                    var code = BestGames.Last().GameToCode();
                    logger.Info($"Best novel game found in generation: {generation}({currentRun}/{runs}):\n{bestGame.GameToString()} \nBest novel game rules found in generation {generation}({currentRun}/{runs}): \n{bestGame.GameToString()}\nWith score: {bestScore}");
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

            for (int k = 0; k < probs.Count; k++)
            {
                int j = cProbSel.BinarySearch2(probs[k]);
                Population[k] = Population[j];
            }
        }
    }
}
