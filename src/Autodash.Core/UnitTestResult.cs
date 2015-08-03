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
            return TestName + " - Passed: " + Passed;
        }
    }
}