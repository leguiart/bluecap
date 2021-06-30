using CCSystem.Lib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCSystem.Lib
{
    public class CCGenericProcessT2<T>
        where T : class
    {
        public IGenerator<T> GenotypeGenerator { get; private set; }
        public IGenerator<T> PhenotypeGenerator { get; private set; }
        public IEvaluator<T> GenotypeEvaluator { get; private set; }
        public IEvaluator<T> PhenotypeEvaluator { get; private set; }

        public CCGenericProcessT2(IGenerator<T> genotypeGenerator, IEvaluator<T> genotypeEvaluator, IEvaluator<T> phenotypeEvaluator, IGenerator<T> phenotypeGenerator = null)
        {
            GenotypeGenerator = genotypeGenerator;
            PhenotypeGenerator = phenotypeGenerator;
            GenotypeEvaluator = genotypeEvaluator;
            PhenotypeEvaluator = phenotypeEvaluator;
        }

        public IEnumerable<T> DoCreativeProcess()
        {
            IEnumerable<T> population = GenotypeGenerator.Populate();           
            population = GenotypeEvaluator.EvaluateAsync(population);
            population = PhenotypeEvaluator.EvaluateAsync(population);
            while (!PhenotypeEvaluator.StopCriteria(population))
            {             
                while (!GenotypeEvaluator.StopCriteria(population))
                {
                    population = GenotypeGenerator.GammaGeneratorFunction(population);
                    population = GenotypeEvaluator.EvaluateAsync(population);
                }
                
                population = PhenotypeEvaluator.EvaluateAsync(population);
                if(PhenotypeGenerator != null)
                    population = PhenotypeGenerator.GammaGeneratorFunction(population);
            }
            return PhenotypeEvaluator.ExtractArtifacts(population);
        }
    }
}
