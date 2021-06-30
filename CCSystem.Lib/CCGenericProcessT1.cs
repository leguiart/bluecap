using CCSystem.Lib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCSystem.Lib
{
    public class CCGenericProcessT1<T>
        where T : class
    {
        public IGenerator<T> Generator { get; private set; }
        public IEvaluator<T> PhenotypeEvaluator { get; private set; }

        public CCGenericProcessT1(IGenerator<T> generator, IEvaluator<T> phenotypeEvaluator)
        {
            Generator = generator;
            PhenotypeEvaluator = phenotypeEvaluator;
        }

        public IEnumerable<T> DoCreativeProcess()
        {
            IEnumerable<T> population = Generator.Populate();
            while (!PhenotypeEvaluator.StopCriteria(population))
            {
                population = PhenotypeEvaluator.EvaluateAsync(population);
                population = Generator.GammaGeneratorFunction(population);
            }
                
            return PhenotypeEvaluator.ExtractArtifacts(population);
        }
    }
}
