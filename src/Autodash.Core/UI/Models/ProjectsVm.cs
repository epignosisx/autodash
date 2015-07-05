using System.Collections.Generic;

namespace Autodash.Core.UI.Models
{
    public class ProjectsVm
    {
        public List<ProjectSuiteRunVm> Projects { get; set; }
    }

    public class ProjectSuiteRunVm
    {
        public Project Project { get; set; }
        public List<ProjectSuiteRunDetail> SuiteRuns { get; set; }
    }

    public class ProjectSuiteRunDetail
    {
        public double DurationMinutes { get; set; }
        public bool Passed { get; set; }

        public override string ToString()
        {
            return (Passed ? "Passed" : "Failed") + ". Took " + DurationMinutes.ToString("0.00") + " mins";
        }
    }
}
