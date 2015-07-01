using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace Autodash.Core
{
    public interface ISuiteRunner
    {
        Task<SuiteRun> Run(SuiteRun run);
    }

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

    public class UnitTestBrowserResult
    {
        public string Browser { get; set; }
        public int Attempt { get; set; }
        public string Stdout { get; set; }
        public string Stderr { get; set; }
        public bool Passed { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        [BsonIgnore]
        public TimeSpan Duration { get { return EndTime - StartTime; } }

        public override string ToString()
        {
            return Browser + " - Passed: " + Passed + ". Attempts: " + Attempt;
        }

    }
}