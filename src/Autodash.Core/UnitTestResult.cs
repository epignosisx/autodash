using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace Autodash.Core
{
    public class UnitTestResult
    {
        public string TestName { get; set; }
        public List<UnitTestBrowserResult> BrowserResults { get; set; }

        [BsonIgnore]
        public TestOutcome Outcome
        {
            get
            {
                var unitTestOutcome = TestOutcome.Inconclusive;
                foreach (var br in BrowserResults.GroupBy(n => n.Browser))
                {
                    //find out the test outcome for each browser
                    var browserTestOutcome = TestOutcome.Inconclusive;
                    foreach (var attempt in br)
                    {
                        if (attempt.Outcome == TestOutcome.Passed)
                        {
                            browserTestOutcome = TestOutcome.Passed;
                            break;
                        }
                        
                        if (attempt.Outcome == TestOutcome.Failed)
                        {
                            browserTestOutcome = TestOutcome.Failed;
                        }
                    }

                    //calculate the overall outcome for all browsers.
                    if (browserTestOutcome == TestOutcome.Failed)
                    {
                        unitTestOutcome = TestOutcome.Failed;
                        break;
                    }
                    
                    if (browserTestOutcome == TestOutcome.Passed)
                    {
                        unitTestOutcome = TestOutcome.Passed;
                    }
                }

                return unitTestOutcome;
            }
        }

        public UnitTestResult()
        {
            BrowserResults = new List<UnitTestBrowserResult>();
        }

        public UnitTestResult(string testName) : this()
        {
            TestName = testName;
        }

        public override string ToString()
        {
            return TestName + " - Outcome: " + Outcome;
        }
    }
}