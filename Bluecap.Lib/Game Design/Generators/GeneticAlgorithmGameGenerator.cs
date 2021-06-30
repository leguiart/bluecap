using Bluecap.Lib.Game_Design.Enums;
using Bluecap.Lib.Game_Design.Evaluators;
using Bluecap.Lib.Game_Design.Interfaces;
using Bluecap.Lib.Game_Model;
using Bluecap.Lib.Utils;
using Bluecap.Lib.Utils.Extensions;
using CCSystem.Lib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        protected List<BaseGame> Population;

        int elitismNum, evaluationsSoFar, totalEvaluations, tournamentSize = 3, tournamentType = 1;
        float bestScore, bestNovelty;
        SelectionMethod selectionMethod;
        BaseGame bestGame, mostNovelGame;

        readonly List<(string, float, float)> BestDistinctGames;
        readonly List<float> bestScoresOfRun = new List<float>(), mostNovelScoresOfRun = new List<float>();      
        readonly Dictionary<string, List<float>> averageScoresOfRun = new Dictionary<string, List<float>>() { {"noveltyScore", new List<float>() }, { "evaluatedScore", new List<float>() }, { "playerBiasScore", new List<float>() }, { "greedIsGoodScore", new List<float>() }, { "skillIsBetterScore", new List<float>() }, { "drawsAreBadScore", new List<float>() }, { "highSkillBalanceScore", new List<float>() }, { "thinkingPaysOutScore", new List<float>() } };
        readonly Dictionary<string, Func<GenerationSettings, dynamic>> funcDict = new Dictionary<string, Func<GenerationSettings, dynamic>>() { {"Direction", GameGenerationUtils.GetDirection }, { "Length", GameGenerationUtils.GetLength }, { "PieceCount", GameGenerationUtils.GetPieceCount }, { "Heading", GameGenerationUtils.GetHeading }, { "Effect", GameGenerationUtils.GetTriggeredEffect } };
        readonly Random rand;

        public GeneticAlgorithmGameGenerator(GenerationSettings settings, IEvaluator<BaseGame> gameEvaluator, GenotypeNoveltyEvaluator noveltyEvaluator, SelectionMethod selectionMethod, int populationSize = 10, int maxIter = 100, int runs = 1, float pc = 0.8f, float pm = 0.01f, float elitism = 0f) : base(settings, gameEvaluator, noveltyEvaluator) 
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
            Population = new List<BaseGame>();
            rand = new Random();
            //NoveltyArchive = new List<(string, float, float)>();
            //this.gameEvaluator = gameEvaluator;
        }

        public override IEnumerable<BaseGame> Populate()
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

        public override IEnumerable<BaseGame> GammaGeneratorFunction(IEnumerable<BaseGame> population)
        {
            Population = population.ToList();
            Select();
            Crossover();
            Mutate();
            return Population;
        }

        public override void StartGenerationProcess()
        {
            logger.Info("Starting ga game generation process...");
            
            for (int run = 1; run <= runs; run++)
            {
                StartingMessage(run, runs);
                logger.Info($"Selection method: {selectionMethod}\n" +
                $"pm: {pm}\n" +
                $"pc: {pc}\n" +
                $"elitismNum: {elitismNum}\n" +
                $"populationSize: {populationSize}\n" +
                $"run {run}/{runs}");
                //Initialize and clean all relevant variables
                evaluationsSoFar = 0;
                bestScore = float.MinValue;
                //Overestimate initial evaluation time as: (100 turns per game) * (max time per turn) * (number of games to test)
                totalEvaluations = populationSize * maxIter;
                //estimatedTimeLeft = 100 * GameEvaluation.instance.TimeAllottedPerTurn * totalEvaluations;
                noveltyEvaluator.NoveltyArchive.Clear();
                Population.Clear();
                BestDistinctGames.Clear();
                Population = Populate().ToList();
                bestScoresOfRun.Clear();
                foreach (var kv in averageScoresOfRun)
                    averageScoresOfRun[kv.Key].Clear();
                int its = 1;


                while (its <= maxIter)
                {
                    Select(run, its);
                    Crossover();
                    Mutate();
                    //noveltyEvaluator.AdjustArchiveSettings();
                    its++;
                }

                logger.Info($"Best scores for run({run}/{runs}): ");
                string bestScores = string.Empty;
                foreach (var score in bestScoresOfRun)
                    bestScores += score.ToString() + ", ";
                bestScores = bestScores.TrimEnd(',', ' ');
                logger.Info(bestScores);

                logger.Info($"Most novel scores for run({run}/{runs}): ");
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
                int j = 0;
                foreach(var elem in noveltyEvaluator.NoveltyArchive)
                    logger.Info($"Novelty archive item[{j}]:\n" +
                        $"{elem.Key}\n" +
                        $"Novelty score = {elem.Value.Item1}\n" +
                        $"Fitness score = {elem.Value.Item2}");

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
                    $"With fitness score {mostNovelGame.evaluatedScore}\n" +
                    $"Novelty score: {bestNovelty}");
                logger.Info($"Generation Process Complete for run ({run}/{runs})!");
                gameTestingFinished = true;
                // Console.WriteLine("Best game score: "+bestScore);
            }

        }

        protected void Crossover()
        {
            int nCross = (Population.Count - elitismNum) / 2;
            //System.Random rand = new System.Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            
            for (int i = 0; i < nCross; i++)
            {
                float p = (float)rand.NextDouble();
                if (p < pc)
                {
                    int cross_point = rand.Next(0, Population[0].Genotype.Count - 1);
                    int genotypeSize = Population[2 * i].Genotype.Count;
                    object[] copy1 = new object[genotypeSize];
                    object[] copy2 = new object[genotypeSize];

                    Population[2 * i].Genotype.CopyTo(copy1);
                    Population[2 * i + 1].Genotype.CopyTo(copy2);

                    List<object> son1 = copy1.Select((s, index) =>
                    {
                        if (index > cross_point)
                            s = copy2[index];
                        return s;
                    }).ToList();
                    List<object> son2 = copy2.Select((s, index) =>
                    {
                        if (index > cross_point)
                            s = copy1[index];
                        return s;
                    }).ToList();

                    Population[2 * i].Genotype = son1;
                    Population[2 * i + 1].Genotype = son2;
                }
            }
        }

        protected void Mutate()
        {
            var flattenedGenotypes = new List<LinkedList<object>>();
            for (int i = 0; i < populationSize - elitismNum; i++)
            {
                flattenedGenotypes.Add(new LinkedList<object>());
                foreach(var gene in Population[i].Genotype)
                {
                    Type geneType = gene.GetType();
                    if(geneType == typeof(int))
                        flattenedGenotypes[i].AddLast(gene);

                        
                    //Flatten conditions
                    if (geneType.IsSubclassOf(typeof(Condition)))
                        AppendElementsFromObject(geneType, gene, flattenedGenotypes[i]);
                    //Flatten list of effects
                    if (geneType == typeof(List<Effect>))
                    {
                        foreach (var el in (List<Effect>)gene)
                            AppendElementsFromObject(el.GetType(), el, flattenedGenotypes[i]);
                    }

                }
            }

            //Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            for(int i = 0; i < flattenedGenotypes.Count; i++)
            {
                var current = flattenedGenotypes[i].First;
                while(current != null)
                {
                    var gene = current.Value;
                    float r = (float)rand.NextDouble();
                    if (r < pm)
                    {
                        Type geneType = gene.GetType();
                        if (geneType.BaseType == typeof(TypeInfo))
                        {
                            object obj = null;
                            if (((Type)gene).IsSubclassOf(typeof(Condition)))
                                obj = GameGenerationUtils.GenerateCondition(settings);
                            else if (((Type)gene).IsSubclassOf(typeof(Effect)))
                            {

                                var existingTypes = flattenedGenotypes[i].Select(s => 
                                {
                                    if (s.GetType().BaseType == typeof(TypeInfo) && ((Type)s).IsSubclassOf(typeof(Effect)) && ((Type)s) != (Type)gene)
                                        return s;
                                    return null;    
                                });
                                obj = GameGenerationUtils.GenerateEffect(settings);
                                Type ot = obj.GetType();
                                while (existingTypes.Contains(ot))
                                {
                                    obj = GameGenerationUtils.GenerateEffect(settings);
                                    ot = obj.GetType();
                                }

                            }
                                
                            var end = ObtainEndingSectionNode(current);
                            current = current.Previous;
                            var begin = current.Next;
                            if (end != null)
                                RemoveElementsBetween(flattenedGenotypes[i], begin, end.Previous);
                            else
                                RemoveElementsAfter(flattenedGenotypes[i], begin);
                            Type t = obj.GetType();
                            AppendElementsAfterNode(t, obj, flattenedGenotypes[i], current);
                            current = current.Next;
                        }

                        if(geneType == typeof(int))
                        {
                            if (current.Previous == null)
                                current.Value = GameGenerationUtils.GenerateRandomWidth(settings);
                            else
                                current.Value = GameGenerationUtils.GenerateRandomHeight(settings);
                        }

                        if (geneType == typeof(ValueTuple<string, object>))
                        {
                            var t = (ValueTuple<string,object>)gene;
                            var tNew = (t.Item1, (object)funcDict[t.Item1](settings));
                            current.Value = tNew;
                        }
                    }
                    current = current.Next;
                }
            }

            for(int i = 0; i < flattenedGenotypes.Count; i++)
            {
                Population[i].Genotype = UnflattenGenotype(flattenedGenotypes[i]);
                var li = new List<Effect>();
                var genotypeCopy = new List<object>(Population[i].Genotype);
                foreach(var gene in genotypeCopy)
                {
                    var t = gene.GetType();
                    if (t.IsSubclassOf(typeof(Effect)))
                    {
                        li.Add((Effect)gene);
                        Population[i].Genotype.Remove(gene);
                    }
                        
                }
                Population[i].Genotype.Add(li);
            }

            //System.Random rand = new System.Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            //for (int i = 0; i < populationSize - elitismNum; i++)
            //{
            //    for(int j = 0; j < Population[i].Genotype.Count; j++)
            //    {
            //        if ((float)rand.NextDouble() < pm)
            //        {
            //            int geneToMutate = j;
            //            GameGenerationUtils.Mutate(geneToMutate, Population[i].Genotype, settings);
            //        }
            //    }
            //}
        }

        private LinkedListNode<object> ObtainEndingSectionNode(LinkedListNode<object> beginSectionNode)
        {
            var endingSectionNode = beginSectionNode.Next;
            var geneType = endingSectionNode.Value.GetType();

            while (endingSectionNode != null && geneType.BaseType != typeof(TypeInfo))
            {
                endingSectionNode = endingSectionNode.Next;
                if (endingSectionNode != null)
                    geneType = endingSectionNode.Value.GetType();
            }
            return endingSectionNode;
        }

        private void AppendElementsFromObject(Type type, object obj, LinkedList<object> linkedList)
        {

            linkedList.AddLast(type);
            foreach (var attribute in type.GetFields())
            {
                var o = attribute.GetValue(obj);
                linkedList.AddLast((attribute.Name, o));
            }
        }

        private void RemoveElementsBetween(LinkedList<object> linkedList, LinkedListNode<object> begin, LinkedListNode<object> end)
        {
            var current = begin.Next;
            linkedList.Remove(begin);
            while (current != end)
            {
                var next = current.Next;
                linkedList.Remove(current);
                current = next;
            }
            linkedList.Remove(current);
        }

        private void RemoveElementsAfter(LinkedList<object> linkedList, LinkedListNode<object> begin)
        {
            var current = begin.Next;
            linkedList.Remove(begin);
            while (current != null)
            {
                var next = current.Next;
                linkedList.Remove(current);
                current = next;
            }
        }

        private void AppendElementsAfterNode(Type type, object obj, LinkedList<object> linkedList, LinkedListNode<object> addAfter)
        {

            linkedList.AddAfter(addAfter, type);
            var next = addAfter.Next;
            foreach (var attribute in type.GetFields())
            {
                var o = attribute.GetValue(obj);
                linkedList.AddAfter(next, (attribute.Name, o));
                next = next.Next;
            }
        }

        private List<object> UnflattenGenotype(LinkedList<object> flattenedGenotype)
        {
            var genotype = new List<object>();
            var current = flattenedGenotype.First;
            while (current != null)
            {
                var obj = current.Value;
                Type geneType = obj.GetType();
                if (geneType == typeof(int))
                    genotype.Add(obj);
                if (geneType.BaseType == typeof(TypeInfo))
                {
                    //Get parameters 
                    var next = current.Next;
                    var previous = current;
                    var pars = new List<object>();
                    while (next != null && next.Value.GetType().BaseType != typeof(TypeInfo))
                    {
                        pars.Add(((ValueTuple<string, object>)next.Value).Item2);
                        next = next.Next;
                        if (next != null)
                            previous = next.Previous;
                    }
                    // get public constructors
                    var ctors = (obj as Type).GetConstructors(BindingFlags.Instance | BindingFlags.Public);
                    // invoke the first public constructor.
                    var o = ctors[0].Invoke(pars.ToArray());
                    genotype.Add(o);
                    current = previous;
                }
                current = current.Next;
            }
            return genotype;
        }

        private void Select()
        {
            Population = Population.OrderBy(s => GetFitnessMetric(s)).ToList();
            switch (selectionMethod)
            {
                case SelectionMethod.PROPORTIONAL:
                    //Fitness proportional selection: Roulette wheel
                    float fitnessSum = Population.Select(s => GetFitnessMetric(s)).Sum();
                    float sum = 0;
                    var cProbSel = Population.Select(s => sum += GetFitnessMetric(s) / fitnessSum).ToList();
                    System.Random rand = new System.Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                    var probs = new List<float>();
                    for (int i = 0; i < Population.Count - elitismNum; i++)
                        probs.Add((float)rand.NextDouble());

                    for (int k = 0; k < probs.Count; k++)
                    {
                        int j = cProbSel.BinarySearch2(probs[k]);
                        Population[k] = Population[j].Copy();
                    }
                    break;
                case SelectionMethod.TOURNAMENT:
                    if (tournamentType == 0)
                    {
                        int t = 0;
                        while (t < Population.Count - elitismNum)
                        {
                            List<int> tournamentContestants = RandomUtils.GenerateRandomPermutation(Population.Count).Take(tournamentSize).ToList();
                            float greatestScoreSoFar = 0f;
                            foreach (var contestant in tournamentContestants)
                            {
                                var ftnssMetric = GetFitnessMetric(Population[contestant]);
                                if (ftnssMetric > greatestScoreSoFar)
                                {
                                    greatestScoreSoFar = ftnssMetric;
                                    Population[t] = Population[contestant].Copy();
                                }
                            }
                            t++;
                        }

                    }
                    else if (tournamentType == 1)
                    {
                        int t = 0;
                        int remainder = (populationSize) % tournamentSize;
                        while (t < Population.Count - elitismNum)
                        {
                            List<int> permutation = RandomUtils.GenerateRandomPermutation(Population.Count).ToList();
                            for (int i = 0; i < permutation.Count && t < Population.Count - elitismNum; i += tournamentSize)
                            {
                                float greatestScoreSoFar = 0f;
                                for (int j = i; j < Math.Min(i + tournamentSize, permutation.Count); j++)
                                {
                                    var ftnssMetric = GetFitnessMetric(Population[permutation[j]]);
                                    if (ftnssMetric > greatestScoreSoFar)
                                    {

                                        greatestScoreSoFar = ftnssMetric;
                                        Population[t] = Population[permutation[j]].Copy();
                                    }
                                }
                                t++;
                            }

                        }
                    }
                    break;
                case SelectionMethod.SUS:
                    sum = 0;
                    fitnessSum = Population.Select(s => GetFitnessMetric(s)).Sum();
                    rand = new System.Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                    var prob = fitnessSum / Population.Count;
                    var start = (float)rand.NextDouble();
                    var pointers = new List<float>();
                    cProbSel = Population.Select(s => sum += GetFitnessMetric(s) / fitnessSum).ToList();
                    for (int i = 0; i < Population.Count - elitismNum; i++)
                        pointers.Add(start * prob + i * prob);
                    for (int i = 0; i < Population.Count - elitismNum; i++)
                    {
                        int j = cProbSel.BinarySearch2(pointers[i]);
                        Population[i] = Population[j].Copy();
                    }
                    break;
            }
        }

        private void Select(int currentRun, int generation)
        {

            //Phenotypic evaluation
            Console.WriteLine($"Starting parallel phenotyptic evaluation for generation: {generation} ({currentRun}/{runs})");
            Population = gameEvaluator.EvaluateAsync(Population).ToList();

            //Genotypic evaluation (novelty considered only)
            Console.WriteLine($"Starting genotype evaluation for generation: {generation} ({currentRun}/{runs})");
            Population = noveltyEvaluator.EvaluateAsync(Population).ToList();
            Console.WriteLine($"Novelty Archive size: {noveltyEvaluator.NoveltyArchive.Count}");

            //Get the average score metrics for each type of matchup, for each generation for data analysis purposes
            var gScores = gameEvaluator.GenerationScores.Select(s => { return new KeyValuePair<string, float>(s.Key, s.Value.Count > 0 ? s.Value.Average() : 0); }).ToDictionary(k => k.Key, v => v.Value);

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
            logger.Info($"Best quality score of generation {generation}({currentRun}/{runs}): {bestScoresOfRun.Last()}");
            logger.Info($"Best novelty score of generation {generation}({currentRun}/{runs}): {mostNovelScoresOfRun.Last()}");
            foreach (var g in gScores)
                averageScoresOfRun[g.Key].Add(g.Value);
            averageScoresOfRun["evaluatedScore"].Add(Population.Select(s => s.evaluatedScore).ToList().Average());
            averageScoresOfRun["noveltyScore"].Add(Population.Select(s => s.noveltyScore).ToList().Average());
            logger.Info($"Average quality of generation {generation}({currentRun}/{runs}): {averageScoresOfRun["evaluatedScore"].Last()}");
            logger.Info($"Average novelty of generation {generation}({currentRun}/{runs}): {averageScoresOfRun["noveltyScore"].Last()}");
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

            Select();


        }

        protected abstract float GetFitnessMetric(BaseGame game);

        protected abstract void StartingMessage(int run, int runs);
    }
}
