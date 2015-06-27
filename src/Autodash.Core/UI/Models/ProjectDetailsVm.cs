using System.Collections.Generic;

namespace Autodash.Core.UI.Models
{
    public class ProjectDetailsVm
    {
        public Project Project { get; set; }
        public List<TestSuite> Suites { get; set; }
        public List<SuiteRun> LastSuiteRuns { get; set; }
    }
}