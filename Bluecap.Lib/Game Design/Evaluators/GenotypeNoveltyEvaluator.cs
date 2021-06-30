using Bluecap.Lib.Game_Design.Interfaces;
using Bluecap.Lib.Game_Model;
using Bluecap.Lib.Utils;
using CCSystem.Lib.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluecap.Lib.Game_Design.Evaluators
{
    public class GenotypeNoveltyEvaluator : IEvaluator<BaseGame>
    {
        public ConcurrentDictionary<string, (float, float)> NoveltyArchive { get; private set; }
        public ConcurrentDictionary<string, ConcurrentBag<float>> GenerationScores { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        float noveltyThreshold, noveltyFloor;
        int maxNoveltyArchiveSize, minNoveltyArchiveSize, kNeighbors, maxIter, itemsAddedInGeneration = 0, timeOut = 0, its = 0;

        public GenotypeNoveltyEvaluator(float noveltyThreshold = 30f, float noveltyFloor = 0.25f, int minNoveltyArchiveSize = 1, int kNeighbors = 20, int maxNoveltyArchiveSize = 25, int maxIter = 100)
        {
            //NoveltyArchive = new List<(string, float, float)>();
            this.noveltyThreshold = noveltyThreshold;
            this.noveltyFloor = noveltyFloor;
            this.minNoveltyArchiveSize = minNoveltyArchiveSize;
            this.kNeighbors = kNeighbors;
            this.maxNoveltyArchiveSize = maxNoveltyArchiveSize;
            this.maxIter = maxIter;
            NoveltyArchive = new ConcurrentDictionary<string, (float, float)>();
        }

        public IEnumerable<BaseGame> Evaluate(IEnumerable<BaseGame> population)
        {
            var poplist = new ConcurrentBag<BaseGame>(population);
            //Parallel.ForEach(population, (organism, _, index) => 
            //{
            //    var gameCode = organism.GameToCode();
            //    organism.noveltyScore = AverageKNNDistance(gameCode, poplist, NoveltyArchive, kNeighbors);
            //    if (organism.noveltyScore > noveltyThreshold || NoveltyArchive.Count < minNoveltyArchiveSize)
            //    {
            //        if (!NoveltyArchive.ContainsKey(gameCode))
            //        {
            //            itemsAddedInGeneration++;
            //            int tries = 0;
            //            bool flag = false;
            //            while (!flag && tries <= 100)
            //            {
            //                tries++;
            //                flag = NoveltyArchive.TryAdd(organism.GameToCode(), (organism.noveltyScore, organism.evaluatedScore));
            //            }
            //            //var NoveltyArchiveArr = NoveltyArchive.ToArray();
            //            //var NoveltyArchiveList = NoveltyArchiveArr.OrderBy(s => s.Value.Item2).ToList();
            //            //var first = NoveltyArchiveList[0].Key;
            //            //var o = (0f, 0f);
            //            //if (NoveltyArchive.Count > maxNoveltyArchiveSize)
            //            //{
            //            //    tries = 0;
            //            //    flag = false;
            //            //    while (!flag && tries <= 100)
            //            //    {
            //            //        tries++;
            //            //        flag = NoveltyArchive.TryRemove(first, out o);
            //            //    }
            //            //}
                            
            //        }

            //    }

            //});
            foreach (var organism in population)
            {
                organism.noveltyScore = AverageKNNDistance(organism.GameToCode(), poplist, NoveltyArchive, kNeighbors);
                if (organism.noveltyScore > noveltyThreshold || NoveltyArchive.Count < minNoveltyArchiveSize)
                {
                    var gameCode = organism.GameToCode();
                    if (!NoveltyArchive.ContainsKey(gameCode))
                    {
                        itemsAddedInGeneration++;
                        NoveltyArchive.TryAdd(organism.GameToCode(), (organism.noveltyScore, organism.evaluatedScore));
                        //NoveltyArchive = NoveltyArchive.OrderBy(s => s.Item2).ToList();
                        //if (NoveltyArchive.Count > maxNoveltyArchiveSize)
                        //    NoveltyArchive.RemoveAt(0);
                    }

                }
            }
            AdjustArchiveSettings();
            return population;
        }

        public IEnumerable<BaseGame> EvaluateAsync(IEnumerable<BaseGame> population)
        {
            var poplist = new ConcurrentBag<BaseGame>(population);
            Parallel.ForEach(population, (organism, _, index) =>
            {
                var gameCode = organism.GameToCode();
                organism.noveltyScore = AverageKNNDistance(gameCode, poplist, NoveltyArchive, kNeighbors);
                if (organism.noveltyScore > noveltyThreshold || NoveltyArchive.Count < minNoveltyArchiveSize)
                {
                    if (!NoveltyArchive.ContainsKey(gameCode))
                    {
                        itemsAddedInGeneration++;
                        int tries = 0;
                        bool flag = false;
                        while (!flag && tries <= 100)
                        {
                            tries++;
                            flag = NoveltyArchive.TryAdd(organism.GameToCode(), (organism.noveltyScore, organism.evaluatedScore));
                        }
                        //var NoveltyArchiveArr = NoveltyArchive.ToArray();
                        //var NoveltyArchiveList = NoveltyArchiveArr.OrderBy(s => s.Value.Item2).ToList();
                        //var first = NoveltyArchiveList[0].Key;
                        //var o = (0f, 0f);
                        //if (NoveltyArchive.Count > maxNoveltyArchiveSize)
                        //{
                        //    tries = 0;
                        //    flag = false;
                        //    while (!flag && tries <= 100)
                        //    {
                        //        tries++;
                        //        flag = NoveltyArchive.TryRemove(first, out o);
                        //    }
                        //}

                    }

                }

            });
            AdjustArchiveSettings();
            its++;
            return population;
        }

        public float AverageKNNDistance(string oganismGameCode, ConcurrentBag<BaseGame> population, ConcurrentDictionary<string, (float, float)> noveltyArchive, int k)
        {
            var distances = new List<float>();
            foreach (var p in population)
            {
                var pGameCode = p.GameToCode();
                if (pGameCode != oganismGameCode)
                    distances.Add(LevenshteinDistance(oganismGameCode, pGameCode));
            }

            foreach (var key in noveltyArchive.Keys)
            {
                distances.Add(LevenshteinDistance(oganismGameCode, key));
            }
            distances = distances.OrderBy(s => s).ToList();
            float avgKnnDist = 0f;
            for (int j = 0; j < k && j < distances.Count; j++)
            {
                avgKnnDist += distances[j];
            }
            avgKnnDist /= Math.Min(k, distances.Count);
            return avgKnnDist;
        }

        public void AdjustArchiveSettings()
        {
            if (itemsAddedInGeneration == 0)
                timeOut++;
            else
                timeOut = 0;
            if (timeOut >= 10)
            {
                noveltyThreshold *= 0.95f;
                noveltyThreshold = Math.Max(noveltyThreshold, noveltyFloor);
                timeOut = 0;
            }
            if (itemsAddedInGeneration >= 4)
                noveltyThreshold *= 1.2f;
            itemsAddedInGeneration = 0;
        }

        private int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        public bool StopCriteria(IEnumerable<BaseGame> population)
        {
            return NoveltyArchive.Keys.Count >= maxNoveltyArchiveSize || its >= maxIter;
        }

        public IEnumerable<BaseGame> ExtractArtifacts(IEnumerable<BaseGame> population)
        {
            float nAvg = population.Select(s => s.noveltyScore).Average();
            return population.Where(s => s.noveltyScore >= nAvg);
        }
    }
}
