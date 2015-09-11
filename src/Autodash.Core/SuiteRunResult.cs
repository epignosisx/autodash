using System.Collections.Generic;
using System.Linq;

namespace Autodash.Core
{
    public class SuiteRunResult
    {
        public TestOutcome Outcome 
        {
            get
            {
                if(CollectionResults == null || CollectionResults.Count == 0)
                    return TestOutcome.Inconclusive;

                foreach (var coll in CollectionResults)
                {
                    if (coll.Outcome == TestOutcome.Failed)
                        return TestOutcome.Failed;
                    if (coll.Outcome == TestOutcome.Inconclusive)
                        return TestOutcome.Inconclusive;
                }
                return TestOutcome.Passed;
            }
        }

        public int PassedTotal
        {
            get
            {
                if (CollectionResults == null)
                    return 0;
                return CollectionResults.SelectMany(n => n.UnitTestResults).Count(n => n.Outcome == TestOutcome.Passed);
            }
        }

        public int FailedTotal
        {
            get
            {
                if (CollectionResults == null)
                    return 0;
                return CollectionResults.SelectMany(n => n.UnitTestResults).Count(n => n.Outcome == TestOutcome.Failed);
            }
        }

        public int InconclusiveTotal
        {
            get
            {
                if (CollectionResults == null)
                    return 0;
                return CollectionResults.SelectMany(n => n.UnitTestResults).Count(n => n.Outcome == TestOutcome.Inconclusive);
            }
        }

        public string Details { get; set; }
        public string Status { get; set; }
        public List<UnitTestCollectionResult> CollectionResults { get; set; }

        public SuiteRunResult()
        {
            CollectionResults = new List<UnitTestCollectionResult>();
        }

        public SuiteRunResult(string details)
        {
            Details = details;
        }
    }
}