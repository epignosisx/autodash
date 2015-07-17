using System.Linq;

namespace Autodash.Core
{
    public class SuiteRunResult
    {
        public bool Passed 
        {
            get { return CollectionResults != null && CollectionResults.All(n => n.Passed); }
        }

        public int PassedTotal
        {
            get
            {
                if (CollectionResults == null)
                    return 0;
                return CollectionResults.SelectMany(n => n.UnitTestResults).Count(n => n.Passed);
            }
        }

        public int FailedTotal
        {
            get
            {
                if (CollectionResults == null)
                    return 0;
                return CollectionResults.SelectMany(n => n.UnitTestResults).Count(n => !n.Passed);
            }
        }

        public string Details { get; set; }
        public string Status { get; set; }
        public UnitTestCollectionResult[] CollectionResults { get; set; }

        public SuiteRunResult()
        {
        }

        public SuiteRunResult(string details)
        {
            Details = details;
        }
    }
}