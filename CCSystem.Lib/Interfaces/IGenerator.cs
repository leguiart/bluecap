using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCSystem.Lib.Interfaces
{
    public interface IGenerator<T>
        where T : class
    {
        IEnumerable<T> GammaGeneratorFunction(IEnumerable<T> population);
        IEnumerable<T> Populate();
    }
}
