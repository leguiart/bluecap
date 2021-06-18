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
    public class GeneticAlgorithmGameGenerator : BaseGameGenerator
    {
        public int populationSize;
        public int maxIter;
        public int runs;
        public float pc, pm;

        public bool parallelEvaluation = false;
        int maxNoveltyArchiveSize = 20, kNeighbors = 5;
        float noveltyThreshold = 15f;
        List<BaseGame> Population;
        List<(string, float)> NoveltyArchive;
        List<string> BestDistinctGames;

        readonly List<float> bestScoresOfRun = new List<float>(), mostNovelScoresOfRun = new List<float>();      
        readonly Dictionary<string, List<float>> averageScoresOfRun = new Dictionary<string, List<float>>() { {"noveltyScore", new List<float>() }, { "evaluatedScore", new List<float>() }, { "playerBiasScore", new List<float>() }, { "greedIsGoodScore", new List<float>() }, { "skillIsBetterScore", new List<float>() }, { "drawsAreBadScore", new List<float>() }, { "highSkillBalanceScore", new List<float>() } };

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
            BestDistinctGames = new List<string>();
            //BestGames = new List<BaseGame>();
            //bestRulesCodes = new List<string>();
            NoveltyArchive = new List<(string, float)>();
            this.gameEvaluator = gameEvaluator;
        }

        //List<GameObject> evaluator;
        private List<BaseGame> Populate()
        {
            //evaluator = new List<GameObject>();
            List<BaseGame> population = new List<BaseGame>();
            while(NoveltyArchive.Count < 15)
            {
                NoveltyArchive.Clear();
                population.Clear();
                for (int i = 0; i < populationSize; i++)
                    population.Add(GameGenerationUtils.GenerateRandomGame(settings));
                //evaluator.Add(new GameObject($"evaluator{i}", typeof(GameEvaluation)));
                _ = EvaluateNovelty(population);
            }
            NoveltyArchive.Clear();

            return population;
        }
        public IEnumerable<BaseGame> EvaluateNovelty(IEnumerable<BaseGame> population)
        {
            int i = 0;
            var poplist = population.ToList();
            foreach(var organism in population)
            {
                organism.noveltyScore = AverageKNNDistance(i, poplist, NoveltyArchive, kNeighbors);
                if(organism.noveltyScore > noveltyThreshold)
                {
                    var gameCode = organism.GameToCode();
                    if(!NoveltyArchive.Select(s => s.Item1).Contains(gameCode))
                    {
                        NoveltyArchive.Add((organism.GameToCode(), organism.noveltyScore));
                        NoveltyArchive = NoveltyArchive.OrderBy(s => s.Item2).ToList();
                        if (NoveltyArchive.Count > maxNoveltyArchiveSize)
                            NoveltyArchive.RemoveAt(0);
                    }

                }
            }
            return population;
        }

        public float AverageKNNDistance(int i, List<BaseGame> population, List<(string, float)> noveltyArchive, int k)
        {
            var distances = new List<(BaseGame, float)>();
            for(int j = 0; j < population.Count; j++)
            {
                if (j != i)
                    distances.Add((population[j],StringUtils.LevenshteinDistance(population[i].GameToCode(), population[j].GameToCode())));
            }
            for (int j = 0; j < noveltyArchive.Count; j++)
            {
                distances.Add((population[j], StringUtils.LevenshteinDistance(population[i].GameToCode(), noveltyArchive[j].Item1)));
            }
            distances = distances.OrderBy(s => s.Item2).ToList();
            float avgKnnDist = 0f;
            for (int j = 0; j < k && j < distances.Count; j++)
            {
                avgKnnDist += distances[j].Item2;
            }
            avgKnnDist /= Math.Min(k, distances.Count);
            return avgKnnDist;
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
                foreach(var kv in averageScoresOfRun)
                {
                    string averageScores = string.Empty;
                    foreach (var avg in kv.Value)
                        averageScores += avg.ToString() + ", ";
                    averageScores = averageScores.TrimEnd(',', ' ');
                    logger.Info($"{kv.Key}:\n{averageScores}");
                }


                logger.Info($"Best game code for ({run}/{runs}):\n{bestGame.GameToCode()}\n" +
                    $"Best game rules for run ({run}/{runs}): \n{bestGame.GameToString()}\n" +
                    $"With score {bestScore}");
                logger.Info($"Generation Process Complete for run ({run}/{runs})!");
                gameTestingFinished = true;
                // Console.WriteLine("Best game score: "+bestScore);
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
            //Genotypic evaluation (novelty considered only)
            Console.WriteLine($"Starting genotype evaluation for generation: {generation} ({currentRun}/{runs})");
            Population = EvaluateNovelty(Population).ToList();
            Console.WriteLine($"Novelty Archive size: {NoveltyArchive.Count}");
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

            //Order population by goal-orienteed fitness (Novelty search terminology) (phenotype evaluation score (Ventura's terminology))
            Population = Population.OrderBy(s => s.evaluatedScore).ToList();
            bestScoresOfRun.Add(Population.Last().evaluatedScore);


            if (Population.Last().evaluatedScore > bestScore)
            {
                bestGame = Population.Last();
                bestScore = bestGame.evaluatedScore;
                bestRulesCode = bestGame.GameToCode();
            }


            foreach (var g in gScores)
                averageScoresOfRun[g.Key].Add(g.Value);
            averageScoresOfRun["evaluatedScore"].Add(Population.Select(s => s.evaluatedScore).ToList().Average());
            averageScoresOfRun["noveltyScore"].Add(Population.Select(s => s.noveltyScore).ToList().Average());

            //Get best game so far we hadn't found up until now
            for (int i = populationSize - 1; i > -1; i--)
            {
                if (!BestDistinctGames.Contains(Population[i].GameToCode()))
                {
                    BestDistinctGames.Add(Population[i].GameToCode());
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


            //Fitness proportional selection: Roulette wheel
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
