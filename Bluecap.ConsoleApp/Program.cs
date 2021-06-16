using Bluecap.Lib.Game_Design.Enums;
using Bluecap.Lib.Game_Design.Evaluators;
using Bluecap.Lib.Game_Design.Generators;
using Bluecap.Lib.Game_Design.Interfaces;
using Bluecap.Lib.Game_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluecap.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            int minBoardDimension = 3;
            int maxBoardDimension = 12;
            bool forceSquareBoard = false;

            int minUpdateEffects = 1;
            int maxUpdateEffects = 3;
            Heading[] allowedFallDirections = new Heading[] {Heading.UP, Heading.DOWN, Heading.RIGHT, Heading.LEFT };
            TriggeredEffect[] allowedTriggeredEffects = new TriggeredEffect[] { TriggeredEffect.DELETE, TriggeredEffect.FLIP, TriggeredEffect.CASCADE};

            bool includeLossCondition = true;
            Direction[] allowedLineDirections = new Direction[] {Direction.LINE, Direction.CARDINAL, Direction.ROW, Direction.COL };
            int minLineLength = 3;
            int maxLineLength = 6;
            int[] pieceCountTargets = new int[] { 5, 10, 15, 18, 20 };
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
            IEvaluator<BaseGame> evaluator = new StandardEvaluator();
            GeneticAlgorithmGameGenerator generator = new GeneticAlgorithmGameGenerator(generationSettings, evaluator, populationSize:20, maxIter:30, runs:3, pm:0.2f);
            generator.StartGenerationProcess();
        }
    }
}
