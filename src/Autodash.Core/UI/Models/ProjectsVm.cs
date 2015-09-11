﻿using System.Collections.Generic;

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
        public TestOutcome Outcome { get; set; }

        public string BgColor()
        {
            switch (Outcome)
            {
                case TestOutcome.Passed: 
                    return "bg-success";
                case TestOutcome.Inconclusive: 
                    return "bg-warning";
            }
            return "bg-danger";
        }

        public override string ToString()
        {
            return Outcome + ". Took " + DurationMinutes.ToString("0.00") + " mins";
        }
    }
}
