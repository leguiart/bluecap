using Bluecap.Lib.Game_Design.Enums;
using Bluecap.Lib.Game_Design.Evaluators;
using Bluecap.Lib.Game_Design.Generators;
using Bluecap.Lib.Game_Design.Interfaces;
using Bluecap.Lib.Game_Model;
using Bluecap.Lib.Utils;
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
            IEvaluator<BaseGame> evaluator = new PhenotypicEvaluator();
            NoveltyEvaluator noveltyEvaluator = new NoveltyEvaluator();
            GeneticAlgorithmGameGenerator generator = null;
            if(args[0] == "goal")
                generator = new GoalOrientedGAGenerator(generationSettings, evaluator, noveltyEvaluator, SelectionMethod.TOURNAMENT, populationSize: 20, maxIter: 30, runs: 3, pm: 0.05f, pc : 0.85f, elitism:0.1f);
            else if(args[0] == "novelty")
                generator = new NoveltyOrientedGAGenerator(generationSettings, evaluator, noveltyEvaluator, SelectionMethod.TOURNAMENT, populationSize: 20, maxIter: 30, runs: 3, pm: 0.05f, pc: 0.85f, elitism: 0.1f);
            generator.StartGenerationProcess();
        }
    }
}
