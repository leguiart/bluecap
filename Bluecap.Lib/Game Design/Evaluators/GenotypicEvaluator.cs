using Bluecap.Lib.Game_Model;
using CCSystem.Lib.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluecap.Lib.Game_Design.Evaluators
{
    public class GenotypicEvaluator : IEvaluator<BaseGame>
    {
        public ConcurrentDictionary<string, ConcurrentBag<float>> GenerationScores { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        IEvaluator<BaseGame> QualityEvaluator, NoveltyEvaluator;
        float alpha, genotypeScoreThresh;
        int maxIter, its = 0;
        public GenotypicEvaluator(IEvaluator<BaseGame> qualityEvaluator, IEvaluator<BaseGame> noveltyEvaluator, float alpha = 0.5f, float genotypeScoreThresh = 0.8f, int maxIter = 100)
        {
            QualityEvaluator = qualityEvaluator;
            NoveltyEvaluator = noveltyEvaluator;
            this.alpha = alpha;
            this.genotypeScoreThresh = genotypeScoreThresh;
            this.maxIter = maxIter;
        }

        public IEnumerable<BaseGame> Evaluate(IEnumerable<BaseGame> population)
        {
            population = QualityEvaluator.Evaluate(population);
            population = NoveltyEvaluator.Evaluate(population);
            var qs = population.Select(s => s.noveltyScore);
            float maxQ = qs.Max();
            float minQ = qs.Min();
            foreach(var p in population)
            {
                var normalizedQ = (p.noveltyScore - minQ) / (maxQ - minQ);
                p.genotypeScore = alpha * p.qualityScore + (1f - alpha) * normalizedQ;
            }
            its++;
            return population;
        }

        public IEnumerable<BaseGame> EvaluateAsync(IEnumerable<BaseGame> population)
        {
            population = QualityEvaluator.Evaluate(population);
            population = NoveltyEvaluator.Evaluate(population);
            var qs = population.Select(s => s.noveltyScore);
            float maxQ = qs.Max();
            float minQ = qs.Min();
            var poplist = population.ToList();
            for(int i = 0; i < poplist.Count; i++)
            {
                if(maxQ != minQ)
                {
                    var normalizedQ = (poplist[i].noveltyScore - minQ) / (maxQ - minQ);
                    poplist[i].genotypeScore = alpha * poplist[i].qualityScore + (1f - alpha) * normalizedQ;
                }
                else
                {
                    poplist[i].genotypeScore = alpha * poplist[i].qualityScore + (1f - alpha);
                }

            }
            its++;
            return poplist;
        }

        public IEnumerable<BaseGame> ExtractArtifacts(IEnumerable<BaseGame> population)
        {
            float gAvg = population.Select(s => s.genotypeScore).Average();
            return population.Where(s => s.genotypeScore >= gAvg);
        }

        public bool StopCriteria(IEnumerable<BaseGame> population)
        {
            float gAvg = population.Select(s => s.genotypeScore).Average();
            bool stopCondition = gAvg >= genotypeScoreThresh && its >= maxIter;
            if(stopCondition)
                its = 0;
            return stopCondition;
        }
    }
}
