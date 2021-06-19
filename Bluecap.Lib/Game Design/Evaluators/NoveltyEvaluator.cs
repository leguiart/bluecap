using Bluecap.Lib.Game_Design.Interfaces;
using Bluecap.Lib.Game_Model;
using Bluecap.Lib.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluecap.Lib.Game_Design.Evaluators
{
    public class NoveltyEvaluator : IEvaluator<BaseGame>
    {
        float noveltyThreshold = 15f;
        int maxNoveltyArchiveSize = 20, kNeighbors = 5;
        public List<(string, float, float)> NoveltyArchive { get; private set; }
        public ConcurrentDictionary<string, ConcurrentBag<float>> GenerationScores { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public NoveltyEvaluator()
        {
            NoveltyArchive = new List<(string, float, float)>();
        }

        public IEnumerable<BaseGame> Evaluate(IEnumerable<BaseGame> population)
        {
            int i = 0;
            var poplist = population.ToList();
            foreach (var organism in population)
            {
                organism.noveltyScore = AverageKNNDistance(i, poplist, NoveltyArchive, kNeighbors);
                if (organism.noveltyScore > noveltyThreshold)
                {
                    var gameCode = organism.GameToCode();
                    if (!NoveltyArchive.Select(s => s.Item1).Contains(gameCode))
                    {
                        NoveltyArchive.Add((organism.GameToCode(), organism.noveltyScore, organism.evaluatedScore));
                        NoveltyArchive = NoveltyArchive.OrderBy(s => s.Item2).ToList();
                        if (NoveltyArchive.Count > maxNoveltyArchiveSize)
                            NoveltyArchive.RemoveAt(0);
                    }

                }
                i++;
            }
            return population;
        }

        public float AverageKNNDistance(int i, List<BaseGame> population, List<(string, float, float)> noveltyArchive, int k)
        {
            var distances = new List<(BaseGame, float)>();
            for (int j = 0; j < population.Count; j++)
            {
                if (j != i)
                    distances.Add((population[j], StringUtils.LevenshteinDistance(population[i].GameToCode(), population[j].GameToCode())));
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

    }
}
