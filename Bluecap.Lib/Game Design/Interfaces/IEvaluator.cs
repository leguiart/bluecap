using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluecap.Lib.Game_Design.Interfaces
{
    public interface IEvaluator<T> where T : class
    {
        IEnumerable<T> Evaluate(IEnumerable<T> population);

    }
}
