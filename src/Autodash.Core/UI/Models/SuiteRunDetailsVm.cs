using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodash.Core.UI.Models
{
    public class SuiteRunDetailsVm
    {
        public SuiteRun SuiteRun { get; set; }
        public Project Project { get; set; }
        public bool DownloadMode { get; set; }
        public bool EmbedResources { get; set; }

        public IEnumerable<UnitTestBrowserResult> GetBrowserResults(UnitTestResult test)
        {
            foreach (var browser in SuiteRun.TestSuiteSnapshot.Configuration.Browsers.OrderBy(n => n))
            {
                var results = test.BrowserResults.Where(n => n.Browser == browser).ToList();
                if (results.Count == 0)
                {
                    yield return new UnitTestBrowserResult
                    {
                        Browser = browser,
                        StartTime = DateTime.MinValue,
                        EndTime = DateTime.MinValue,
                        Outcome = TestOutcome.Failed,
                        Stderr = "Test did not run"
                    };
                }
                else
                {
                    var passedTest = results.FirstOrDefault(n => n.Outcome == TestOutcome.Passed);
                    if (passedTest != null)
                    {
                        yield return passedTest;
                    }
                    else
                    {
                        yield return results.FirstOrDefault();
                    }
                }
            }
        }
    }
}