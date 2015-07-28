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
        public bool Passed
        {
            get
            {
                return BrowserResults.GroupBy(b => b.Browser).All(n => n.Any(p => p.Passed));
            }
        }

        [BsonIgnore]
        private readonly List<string> _ongoingBrowserTests;

        public UnitTestResult()
        {
            BrowserResults = new List<UnitTestBrowserResult>();
            _ongoingBrowserTests = new List<string>(4);
        }

        public UnitTestResult(string testName) : this()
        {
            TestName = testName;
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

            List<string> ongoingSnapshot;
            lock (_ongoingBrowserTests)
            {
                ongoingSnapshot = _ongoingBrowserTests.ToList();
            }

            foreach(var browser in browsers)
            {
                if (ongoingSnapshot.Contains(browser))
                    continue;

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

        public void AddOngoingBrowserTest(string browser)
        {
            lock(_ongoingBrowserTests)
                _ongoingBrowserTests.Add(browser);
        }

        public void RemoveOngoingBrowserTest(string browser)
        {
            lock (_ongoingBrowserTests)
                _ongoingBrowserTests.Remove(browser);
        }

        public override string ToString()
        {
            return TestName + " - Passed: " + Passed;
        }
    }
}