using System.Collections.Generic;
using System.Linq;

namespace Autodash.Core
{
    public class UnitTestCollectionResult
    {
        public string AssemblyName { get; set; }
        public List<UnitTestResult> UnitTestResults { get; set; }

        public bool Passed 
        {
            get { return UnitTestResults.All(n => n.Passed); }
        }
    }
}
