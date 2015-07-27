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

        public IEnumerable<PendingTestRunInfo> GetPendingTestsToRun(string[] browsers, int retryAttempts)
        {
            foreach (var test in UnitTestResults)
            {
                var pendingBrowsers = test.GetPendingBrowserResults(browsers, retryAttempts)
                                          .ToArray();
                if(pendingBrowsers.Length > 0)
                {
                    yield return new PendingTestRunInfo(test, pendingBrowsers);
                }
            }
        }
    }

    public class PendingTestRunInfo
    {
        public UnitTestResult UnitTestResult { get; private set; }
        public string[] Browsers { get; private set; }

        public PendingTestRunInfo(UnitTestResult unitTestResult, string[] browsers)
        {
            UnitTestResult = unitTestResult;
            Browsers = browsers;
        }
    }
}
