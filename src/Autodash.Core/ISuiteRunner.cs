using System;
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
        public List<UnitTestBrowserResult> Browsers { get; set; }
    }

    public class UnitTestBrowserResult
    {
        public string Browser { get; set; }
        public int Attempt { get; set; }
        public string Stdout { get; set; }
        public string Stderr { get; set; }
        public bool Success { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}