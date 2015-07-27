using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace Autodash.Core
{
    public class UnitTestResult
    {
        public string TestName { get; set; }
        public List<UnitTestBrowserResult> BrowserResults { get; set; }

        public UnitTestResult()
        {
            BrowserResults = new List<UnitTestBrowserResult>();
        }

        public UnitTestResult(string testName) : this()
        {
            TestName = testName;
        }

        [BsonIgnore]
        public bool Passed
        {
            get
            {
                return BrowserResults.GroupBy(b => b.Browser).All(n => n.Any(p => p.Passed));
            }
        }

        public UnitTestBrowserResult this[string browser]
        {
            get
            {
                UnitTestBrowserResult result = null;
                foreach (var br in BrowserResults)
                {
                    if (br.Browser == browser)
                    {
                        result = br;
                        if (br.Passed)
                            return br;
                    }
                }
                return result;
            }
        }

        public IEnumerable<string> GetPendingBrowserResults(string[] browsers, int retryAttempts)
        {
            List<UnitTestBrowserResult> snapshot;
            lock (BrowserResults)
            {
                snapshot = BrowserResults.ToList();
            }

            foreach(var browser in browsers)
            {
                var results = snapshot.Where(n => n.Browser == browser).ToList();
                if (results.Count == 0)
                {
                    yield return browser;
                    continue;
                }

                bool passed = results.Any(n => n.Passed);
                if (passed)
                    continue;

                if (results.Count < retryAttempts)
                    yield return browser;
            }
        }

        public override string ToString()
        {
            return TestName + " - Passed: " + Passed;
        }
    }
}