using Bluecap.Lib.Game_Design.Enums;
using Bluecap.Lib.Game_Design.Evaluators;
using Bluecap.Lib.Game_Design.Interfaces;
using Bluecap.Lib.Game_Model;
using Bluecap.Lib.Utils;
using Bluecap.Lib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluecap.Lib.Game_Design.Generators
{
    public abstract class GeneticAlgorithmGameGenerator : BaseGameGenerator
    {
        public int populationSize;
        public int maxIter;
        public int runs;
        public float pc, pm;
        public bool parallelEvaluation = false;

        int elitismNum, evaluationsSoFar, totalEvaluations, tournamentSize = 4;
        float bestScore, bestNovelty;
        SelectionMethod selectionMethod;
        BaseGame bestGame, mostNovelGame;
        List<BaseGame> Population;


        readonly List<(string, float, float)> BestDistinctGames;
        readonly List<float> bestScoresOfRun = new List<float>(), mostNovelScoresOfRun = new List<float>();      
        readonly Dictionary<string, List<float>> averageScoresOfRun = new Dictionary<string, List<float>>() { {"noveltyScore", new List<float>() }, { "evaluatedScore", new List<float>() }, { "playerBiasScore", new List<float>() }, { "greedIsGoodScore", new List<float>() }, { "skillIsBetterScore", new List<float>() }, { "drawsAreBadScore", new List<float>() }, { "highSkillBalanceScore", new List<float>() } };


        

        public GeneticAlgorithmGameGenerator(GenerationSettings settings, IEvaluator<BaseGame> gameEvaluator, NoveltyEvaluator noveltyEvaluator, SelectionMethod selectionMethod, int populationSize = 10, int maxIter = 100, int runs = 1, float pc = 0.8f, float pm = 0.01f, float elitism = 0f) : base(settings, gameEvaluator, noveltyEvaluator) 
        {
            this.populationSize = populationSize;
            this.maxIter = maxIter;
            this.runs = runs;
            this.pc = pc;
            this.pm = pm;
            this.selectionMethod = selectionMethod;
            elitismNum = (int)Math.Floor(elitism * populationSize);
            BestDistinctGames = new List<(string, float, float)>();
            //BestGames = new List<BaseGame>();
            //bestRulesCodes = new List<string>();
            Population = new List<BaseGame>();
            //NoveltyArchive = new List<(string, float, float)>();
            //this.gameEvaluator = gameEvaluator;
        }

        private List<BaseGame> Populate()
        {
            //evaluator = new List<GameObject>();
            List<BaseGame> population = new List<BaseGame>();
            //while(noveltyEvaluator.NoveltyArchive.Count < 5)
            //{
                //noveltyEvaluator.NoveltyArchive.Clear();
                //population.Clear();
                for (int i = 0; i < populationSize; i++)
                    population.Add(GameGenerationUtils.GenerateRandomGame(settings));
                //evaluator.Add(new GameObject($"evaluator{i}", typeof(GameEvaluation)));
            //    _ = noveltyEvaluator.Evaluate(population);
            //}
            //noveltyEvaluator.NoveltyArchive.Clear();

            return population;
        }

        public override void StartGenerationProcess()
        {
            logger.Info("Starting game generation run...");
            for (int run = 1; run <= runs; run++)
            {

                bestScoresOfRun.Clear();
                foreach(var kv in averageScoresOfRun)
                    averageScoresOfRun[kv.Key].Clear();
                evaluationsSoFar = 0;
                bestScore = float.MinValue;
                //Overestimate initial evaluation time as: (100 turns per game) * (max time per turn) * (number of games to test)
                totalEvaluations = populationSize * maxIter;
                //estimatedTimeLeft = 100 * GameEvaluation.instance.TimeAllottedPerTurn * totalEvaluations;
                noveltyEvaluator.NoveltyArchive.Clear();
                Population.Clear();
                BestDistinctGames.Clear();
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

                logger.Info($"Novelty scores for run({run}/{runs}): ");
                bestScores = string.Empty;               
                foreach(var score in mostNovelScoresOfRun)
                    bestScores += score.ToString() + ", ";
                bestScores = bestScores.TrimEnd(',', ' ');
                logger.Info(bestScores);

                logger.Info($"Average scores for run({run}/{runs}): ");
                foreach(var kv in averageScoresOfRun)
                {
                    string averageScores = string.Empty;
                    foreach (var avg in kv.Value)
                        averageScores += avg.ToString() + ", ";
                    averageScores = averageScores.TrimEnd(',', ' ');
                    logger.Info($"{kv.Key}:\n{averageScores}");
                }

                logger.Info($"Novelty archive for run({run}/{runs}): ");
                for (int i = 0; i < noveltyEvaluator.NoveltyArchive.Count; i++)
                    logger.Info($"Novelty archive item[{i}]:\n" +
                        $"{noveltyEvaluator.NoveltyArchive[i].Item1}\n" +
                        $"Novelty score = {noveltyEvaluator.NoveltyArchive[i].Item2}\n" +
                        $"Fitness score = {noveltyEvaluator.NoveltyArchive[i].Item3}");

                logger.Info($"Fittest distinct games for run({run}/{runs}): ");
                for (int i = 0; i < BestDistinctGames.Count; i++)
                    logger.Info($"Best distinct game item[{i}]:\n" +
                        $"{BestDistinctGames[i].Item1}\n" +
                       $"Novelty score = {BestDistinctGames[i].Item2}\n" +
                        $"Fitness score = {BestDistinctGames[i].Item3}");

                logger.Info($"Best game code for ({run}/{runs}):\n{bestGame.GameToCode()}\n" +
                    $"Best game rules for run ({run}/{runs}): \n{bestGame.GameToString()}\n" +
                    $"With fitness score: {bestScore}\n" +
                    $"Novelty score: {bestGame.noveltyScore}");

                logger.Info($"Most novel game code for ({run}/{runs}):\n{mostNovelGame.GameToCode()}\n" +
                    $"Most novel game rules for run ({run}/{runs}): \n{mostNovelGame.GameToString()}\n" +
                    $"With fitness score {bestGame.evaluatedScore}" +
                    $"Novelty score: {bestNovelty}");
                logger.Info($"Generation Process Complete for run ({run}/{runs})!");
                gameTestingFinished = true;
                // Console.WriteLine("Best game score: "+bestScore);
            }

        }

        private void Crossover()
        {
            int nCross = (Population.Count - elitismNum) / 2;
            System.Random rand = new System.Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            for (int i = 0; i < nCross; i++)
            {
                float p = (float)rand.NextDouble();
                if (p < pc)
                {
                    int cross_point = rand.Next(0, Population[0].Genotype.Count - 1);
                    List<object> son1 = Population[2 * i + elitismNum].Genotype.Select((s, index) =>
                    {
                        if (index > cross_point)
                            s = Population[2 * i + elitismNum + 1].Genotype[index];
                        return s;
                    }).ToList();
                    List<object> son2 = Population[2 * i + elitismNum + 1].Genotype.Select((s, index) =>
                    {
                        if (index > cross_point)
                            s = Population[2 * i + elitismNum].Genotype[index];
                        return s;
                    }).ToList();

                    Population[2 * i + elitismNum].Genotype = son1;
                    Population[2 * i + elitismNum + 1].Genotype = son2;
                }
            }
        }

        private void Mutate()
        {
            System.Random rand = new System.Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            for (int i = 0; i < populationSize - elitismNum; i++)
            {
                for(int j = 0; j < Population[i].Genotype.Count; j++)
                {
                    if ((float)rand.NextDouble() < pm)
                    {
                        int geneToMutate = j;
                        GameGenerationUtils.Mutate(geneToMutate, Population[i].Genotype, settings);
                    }
                }

            }
        }

        private void Select(int currentRun, int generation)
        {
            //Genotypic evaluation (novelty considered only)
            Console.WriteLine($"Starting genotype evaluation for generation: {generation} ({currentRun}/{runs})");
            Population = noveltyEvaluator.Evaluate(Population).ToList();
            Console.WriteLine($"Novelty Archive size: {noveltyEvaluator.NoveltyArchive.Count}");
            //Phenotypic evaluation
            Console.WriteLine($"Starting parallel phenotyptic evaluation for generation: {generation} ({currentRun}/{runs})");
            Population = gameEvaluator.Evaluate(Population).ToList();

            //Get the average score metrics for each type of matchup, for each generation for data analysis purposes
            var gScores = gameEvaluator.GenerationScores.Select(s => { return new KeyValuePair<string, float>(s.Key, s.Value.Average()); }).ToDictionary(k => k.Key, v => v.Value);

            //evaluationsSoFar += Population.Count;
            //bestRulesCodes.Clear();


            //Order population by novelty score (Novelty search terminology) (genotype novelty evaluation score (Ventura's terminology))
            Population = Population.OrderBy(s => s.noveltyScore).ToList();
            mostNovelScoresOfRun.Add(Population.Last().noveltyScore);
            if (Population.Last().noveltyScore > bestNovelty)
            {
                mostNovelGame = Population.Last();
                bestNovelty = mostNovelGame.noveltyScore;
            }

            //Order population by goal-orienteed fitness (Novelty search terminology) (phenotype evaluation score (Ventura's terminology))
            Population = Population.OrderBy(s => s.evaluatedScore).ToList();
            bestScoresOfRun.Add(Population.Last().evaluatedScore);
            if (Population.Last().evaluatedScore > bestScore)
            {
                bestGame = Population.Last();
                bestScore = bestGame.evaluatedScore;
            }


            foreach (var g in gScores)
                averageScoresOfRun[g.Key].Add(g.Value);
            averageScoresOfRun["evaluatedScore"].Add(Population.Select(s => s.evaluatedScore).ToList().Average());
            averageScoresOfRun["noveltyScore"].Add(Population.Select(s => s.noveltyScore).ToList().Average());

            //Get best game so far we hadn't found up until now
            var bestDistinctGamesCodes = BestDistinctGames.Select(s => s.Item1);
            for (int i = populationSize - 1; i > -1; i--)
            {
                if (!bestDistinctGamesCodes.Contains(Population[i].GameToCode()))
                {
                    BestDistinctGames.Add((Population[i].GameToCode(), Population[i].noveltyScore, Population[i].evaluatedScore));
                    logger.Info($"Best distinct game code in generation: {generation}({currentRun}/{runs}):\n{bestGame.GameToCode()}\n" +
                        $"Best distinct game rules found in generation {generation}({currentRun}/{runs}): \n{bestGame.GameToString()}\n" +
                        $"With score: {bestScore}");
                    break;
                }

            }

            //BestGames = BestGames.OrderBy(s => s.evaluatedScore).ToList();
            ////if (BestGames.Count > populationSize)
            ////    BestGames.RemoveAt(0);
            //foreach (var bestGame in BestGames)
            //    bestRulesCodes.Add(bestGame.GameToCode());

            switch (selectionMethod)
            {
                case SelectionMethod.PROPORTIONAL:
                    //Fitness proportional selection: Roulette wheel
                    float fitnessSum = Population.Select(s => GetFitnessMetric(s)).Sum();
                    float sum = 0;
                    var cProbSel = Population.Select(s => sum += GetFitnessMetric(s) / fitnessSum).ToList();
                    System.Random rand = new System.Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                    var probs = new List<float>();
                    for (int i = 0; i < Population.Count() - elitismNum; i++)
                        probs.Add((float)rand.NextDouble());

                    for (int k = 0; k < probs.Count - elitismNum; k++)
                    {
                        int j = cProbSel.BinarySearch2(probs[k]);
                        Population[k] = Population[j];
                    }
                    break;
                case SelectionMethod.TOURNAMENT:
                    int t = 0;
                    while(t < Population.Count - elitismNum)
                    {
                        List<int> tournamentContestants = RandomUtils.GenerateRandomPermutation(Population.Count).Take(tournamentSize).ToList();
                        float greatestScoreSoFar = 0f;
                        foreach(var contestant in tournamentContestants)
                        {
                            var ftnssMetric = GetFitnessMetric(Population[contestant]);
                            if (ftnssMetric > greatestScoreSoFar)
                            {
                                greatestScoreSoFar = ftnssMetric;
                                Population[t] = Population[contestant];
                            }
                        }
                    }
                    break;
                case SelectionMethod.SUS:
                    sum = 0;
                    fitnessSum = Population.Select(s => GetFitnessMetric(s)).Sum();
                    rand = new System.Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                    var prob = fitnessSum / (Population.Count - elitismNum);
                    var start = (float)rand.NextDouble();
                    var pointers = new List<float>();
                    cProbSel = Population.Select(s => sum += GetFitnessMetric(s) / fitnessSum).ToList();
                    for (int i = 0; i < Population.Count; i++)
                        pointers.Add(start * prob + i * prob);
                    for (int i = 0; i <  Population.Count - elitismNum; i++)
                    {
                        int j = cProbSel.BinarySearch2(pointers[i]);
                        Population[i] = Population[j];
                    }
                    break;
            }

        }

        protected abstract float GetFitnessMetric(BaseGame game);
    }
}
