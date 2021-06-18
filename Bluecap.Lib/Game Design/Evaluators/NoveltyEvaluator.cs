using Bluecap.Lib.Game_Design.Interfaces;
using Bluecap.Lib.Game_Model;
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
        public ConcurrentDictionary<string, ConcurrentBag<float>> GenerationScores { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IEnumerable<BaseGame> Evaluate(IEnumerable<BaseGame> population)
        {
            throw new NotImplementedException();
        }
    }
}
