using Bluecap.Lib.Game_Design.Enums;
using Bluecap.Lib.Game_Design.Evaluators;
using Bluecap.Lib.Game_Design.Generators;
using Bluecap.Lib.Game_Design.Interfaces;
using Bluecap.Lib.Game_Model;
using Bluecap.Lib.Utils;
using CCSystem.Lib;
using CCSystem.Lib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluecap.ConsoleApp
{
    class Program
    {
        static void CalculateDistances(List<string> sList)
        {
            Console.WriteLine("Levenshtein Distances:");
            for (int i = 0; i < sList.Count; i++)
                for (int j = i + 1; j < sList.Count; j++)
                    Console.WriteLine($"{i},{j}: {StringUtils.LevenshteinDistance(sList[i], sList[j])}");
            Console.WriteLine("Damerau-Levenshtein Distances:");
            for (int i = 0; i < sList.Count; i++)
                for (int j = i + 1; j < sList.Count; j++)
                    Console.WriteLine($"{i},{j}: {StringUtils.GetDamerauLevenshteinDistance(sList[i], sList[j])}");
        }
        static void Main(string[] args)
        {
            //            List<string> sList = new List<string> { 
            //                @"BOARD 5 6
            //FALL DOWN
            //WIN MATCH LINE 3
            //LOSE MATCH LINE 3",
            //                @"BOARD 4 6
            //CAP FLIP
            //WIN COUNT 15
            //LOSE COUNT 5",
            //                @"BOARD 4 5
            //MATCH LINE 3 FLIP
            //CAP FLIP
            //WIN MATCH CARDINAL 4
            //LOSE MATCH LINE 3",
            //                @"BOARD 5 5
            //FALL LEFT
            //WIN MATCH COL 4
            //LOSE MATCH COL 4",
            //                @"BOARD 5 5
            //FALL LEFT
            //WIN COUNT 10
            //LOSE MATCH COL 4",
            //                @"BOARD 4 5
            //FALL LEFT
            //WIN COUNT 10
            //LOSE MATCH COL 4",
            //                @"BOARD 7 7
            //CAP DELETE
            //WIN MATCH LINE 3
            //LOSE MATCH LINE 3",
            //                @"BOARD 9 7
            //CAP DELETE
            //WIN MATCH LINE 3
            //LOSE MATCH LINE 3",
            //                @"BOARD 9 7
            //CAP DELETE
            //WIN MATCH LINE 3
            //LOSE MATCH LINE 3"
            //            };
            //            CalculateDistances(sList);

            int minBoardDimension = 3;
            int maxBoardDimension = 12;
            bool forceSquareBoard = false;

            int minUpdateEffects = 1;
            int maxUpdateEffects = 3;
            Heading[] allowedFallDirections = new Heading[] { Heading.UP, Heading.DOWN, Heading.RIGHT, Heading.LEFT };
            TriggeredEffect[] allowedTriggeredEffects = new TriggeredEffect[] { TriggeredEffect.DELETE, TriggeredEffect.FLIP, TriggeredEffect.CASCADE };

            bool includeLossCondition = true;
            Direction[] allowedLineDirections = new Direction[] { Direction.LINE, Direction.CARDINAL, Direction.ROW, Direction.COL };
            int minLineLength = 3;
            int maxLineLength = 6;
            int[] pieceCountTargets = new int[] { 5, 10, 15, 20, 25 };
            GenerationSettings generationSettings = new GenerationSettings()
            {
                minBoardDimension = minBoardDimension,
                maxBoardDimension = maxBoardDimension,
                forceSquareBoard = forceSquareBoard,
                minUpdateEffects = minUpdateEffects,
                maxUpdateEffects = maxUpdateEffects,
                allowedFallDirections = allowedFallDirections,
                allowedTriggeredEffects = allowedTriggeredEffects,
                includeLossCondition = includeLossCondition,
                allowedLineDirections = allowedLineDirections,
                minLineLength = minLineLength,
                maxLineLength = maxLineLength,
                pieceCountTargets = pieceCountTargets,
            };
            bool rr = args.Contains("rr");
            bool gr = args.Contains("gr");
            bool mg = args.Contains("mg");
            bool mctsMcts = args.Contains("mm");
            bool mctsMctsSkilled = args.Contains("mml");
            SelectionMethod selection = (SelectionMethod)(Convert.ToInt32(args[0]));
            int populationSize = Convert.ToInt32(args[1]);
            int maxIter = Convert.ToInt32(args[2]);
            int runs = Convert.ToInt32(args[3]);
            float pm = (float)Convert.ToDouble(args[4]);
            float pc = (float)Convert.ToDouble(args[5]);
            float elitism = (float)Convert.ToDouble(args[6]);
            GenotypeNoveltyEvaluator noveltyEvaluator = new GenotypeNoveltyEvaluator();
            GenotypeQualityEvaluator qualityEvaluator = new GenotypeQualityEvaluator();
            IEvaluator<BaseGame> evaluator = new PhenotypicEvaluator(noveltyEvaluator, maxIter: maxIter, randomRandom : rr, randomGreedy: gr, mctsGreedy:mg, mctsMcts: mctsMcts, mctsMctsSkilled: mctsMctsSkilled);
            GenotypicEvaluator genotypicEvaluator = new GenotypicEvaluator(qualityEvaluator, noveltyEvaluator, maxIter: maxIter, genotypeScoreThresh:0.95f);
            

            GeneticAlgorithmGameGenerator generator = null;
            //var parametersToTest = new List<(float, float, float)>() { (0.15f, 0.7f, 0.1f), (0.15f, 0.7f, 0f), (0.2f, 0.7f, 0.1f), (0.2f, 0.7f, 0f), (0.15f, 0.75f, 0.1f), (0.15f, 0.75f, 0f), (0.2f, 0.75f, 0.1f), (0.2f, 0.75f, 0f) };
            //var parametersToTest = new List<(float, float, float)>() { (0.1f, 0.8f, 0.1f), (0.1f, 0.8f, 0f), (0.2f, 0.8f, 0.1f), (0.2f, 0.8f, 0f), (0.1f, 0.85f, 0.1f), (0.1f, 0.85f, 0f), (0.2f, 0.85f, 0.1f), (0.2f, 0.85f, 0f) };
            if(args[0] == "param_select")
            {
                var parametersToTest = new List<(float, float, float)>() { (0.1f, 0.8f, 0.1f), (0.2f, 0.8f, 0.1f), (0.1f, 0.85f, 0.1f), (0.2f, 0.85f, 0.1f) };
                var selectionMethods = new SelectionMethod[] { SelectionMethod.PROPORTIONAL, SelectionMethod.TOURNAMENT, SelectionMethod.SUS };
                foreach (var selectionMethod in selectionMethods)
                {
                    foreach (var param in parametersToTest)
                    {
                        if (args[1] == "goal")
                            generator = new GoalOrientedGAGenerator(generationSettings, evaluator, noveltyEvaluator, selectionMethod, populationSize: 11, maxIter: 5, runs: 1, pm: param.Item1, pc: param.Item2, elitism: param.Item3);
                        //generator.StartGenerationProcess();
                        else if (args[1] == "novelty")
                            generator = new NoveltyOrientedGAGenerator(generationSettings, evaluator, noveltyEvaluator, selectionMethod, populationSize: 48, maxIter: 200, runs: 2, pm: param.Item1, pc: param.Item2, elitism: param.Item3);
                        generator.StartGenerationProcess();
                    }

                }

                generator.StartGenerationProcess();
            }
            else if(args.Contains("experiments"))
            {
                if(args.Contains("goal"))
                {
                    generator = new GoalOrientedGAGenerator(generationSettings, evaluator, noveltyEvaluator, selection, populationSize: populationSize, maxIter: maxIter, runs: runs, pm: pm, pc: pc, elitism: elitism);
                    generator.StartGenerationProcess();
                    return;
                }
                else if(args.Contains("novelty")) 
                {
                    generator = new NoveltyOrientedGAGenerator(generationSettings, evaluator, noveltyEvaluator, selection, populationSize: populationSize, maxIter: maxIter, runs: runs, pm: pm, pc: pc, elitism: elitism);
                    generator.StartGenerationProcess();
                    return;
                }else if (args.Contains("CC"))
                {
                    evaluator = new PhenotypicEvaluator(noveltyEvaluator, maxIter: maxIter, randomRandom: rr, randomGreedy: gr, mctsGreedy: mg, mctsMcts: mctsMcts, mctsMctsSkilled: mctsMctsSkilled, logs:true);
                    GeneticAlgorithmGameGenerator generator2 = new GoalOrientedGAGenerator(generationSettings, evaluator, noveltyEvaluator, selection, populationSize: populationSize, maxIter: maxIter, runs: runs, pm: 0f, pc: pc, elitism: elitism);
                    generator = new GenotypeGAGenerator(generationSettings, evaluator, noveltyEvaluator, selection, populationSize: populationSize, maxIter: maxIter, runs: runs, pm: pm, pc: pc, elitism: elitism);
                    CCGenericProcessT2<BaseGame> ccSystem = new CCGenericProcessT2<BaseGame>(generator, genotypicEvaluator, evaluator);
                    ccSystem.DoCreativeProcess();
                    return;
                }

                generator = new GoalOrientedGAGenerator(generationSettings, evaluator, noveltyEvaluator, selection, populationSize: populationSize, maxIter: maxIter, runs: runs, pm: pm, pc: pc, elitism: elitism); ;
                generator.StartGenerationProcess();
                generator = new NoveltyOrientedGAGenerator(generationSettings, evaluator, noveltyEvaluator, selection, populationSize: populationSize, maxIter: maxIter, runs: runs, pm: pm, pc: pc, elitism: elitism); ;
                generator.StartGenerationProcess();
            }

        }
    }
}
