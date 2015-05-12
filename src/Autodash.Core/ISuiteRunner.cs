using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Autodash.Core
{
    public interface ISuiteRunner
    {
        Task<SuiteRun> Run(SuiteRun run);
    }

    public class UnitTestResult
    {
        public string TestName { get; set; }
        public List<UnitTestBrowserResult> Browsers { get; set; }

        public bool Passed
        {
            get
            {
                return Browsers.GroupBy(b => b.Browser).All(n => n.Any(p => p.Passed));
            }
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
    }
}