using Bluecap.Lib.Game_Design.Enums;
using Bluecap.Lib.Game_Design.Evaluators;
using Bluecap.Lib.Game_Design.Interfaces;
using Bluecap.Lib.Game_Model;
using CCSystem.Lib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluecap.Lib.Game_Design.Generators
{
    public class GenotypeGAGenerator : GeneticAlgorithmGameGenerator
    {
        public GenotypeGAGenerator(GenerationSettings settings, IEvaluator<BaseGame> gameEvaluator, GenotypeNoveltyEvaluator noveltyEvaluator, SelectionMethod selectionMethod, int populationSize = 10, int maxIter = 100, int runs = 1, float pc = 0.8F, float pm = 0.01F, float elitism = 0) 
            : base(settings, gameEvaluator, noveltyEvaluator, selectionMethod, populationSize, maxIter, runs, pc, pm, elitism)
        {}

        protected override float GetFitnessMetric(BaseGame game)
        {
            return game.genotypeScore;
        }

        protected override void StartingMessage(int run, int runs)
        {
            logger.Info($"Starting goal oriented generator ({run}/{runs})");
        }
    }
}
