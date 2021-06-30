using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCSystem.Lib.Interfaces
{
    public interface IEvaluator<T> where T : class
    {
        IEnumerable<T> Evaluate(IEnumerable<T> population);

        IEnumerable<T> EvaluateAsync(IEnumerable<T> population);

        IEnumerable<T> ExtractArtifacts(IEnumerable<T> population);

        bool StopCriteria(IEnumerable<T> population);

        ConcurrentDictionary<string, ConcurrentBag<float>> GenerationScores { get; set; }
    }
}
