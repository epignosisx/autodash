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

        public override string ToString()
        {
            return TestName + " - Passed: " + Passed;
        }
    }
}