using System.Collections.Generic;

namespace Autodash.Core.UI.Models
{
    public class ProjectDetailsVm
    {
        public Project Project { get; set; }
        public List<ProjectTestSuiteVm> Suites { get; set; }
    }

    public class ProjectTestSuiteVm
    {
        public TestSuite Suite { get; set; }
        public List<SuiteRun> LastSuiteRuns { get; set; }
    }
}