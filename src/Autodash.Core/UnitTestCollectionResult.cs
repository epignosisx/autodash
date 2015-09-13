using System.Collections.Generic;

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

        public TestOutcome Outcome
        {
            get
            {
                if (UnitTestResults.Count == 0)
                    return TestOutcome.Inconclusive;

                foreach (var result in UnitTestResults)
                {
                    if(result.Outcome == TestOutcome.Failed)
                        return TestOutcome.Failed;
                    if (result.Outcome == TestOutcome.Inconclusive)
                        return TestOutcome.Inconclusive;
                }
                return TestOutcome.Passed;
            }
        }
    }
}
