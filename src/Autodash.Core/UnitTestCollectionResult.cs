using System.Collections.Generic;
using System.Linq;

namespace Autodash.Core
{
    public class UnitTestCollectionResult
    {
        public string AssemblyName { get; set; }
        public List<UnitTestResult> UnitTestResults { get; set; }

        public UnitTestCollectionResult()
        {
            UnitTestResults = new List<UnitTestResult>();
        }

        public bool Passed 
        {
            get { return UnitTestResults.All(n => n.Passed); }
        }
    }
}
