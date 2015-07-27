using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodash.Core
{
    public class TestSuiteConfiguration
    {
        public string[] Browsers { get; set; }
        public string TestTagsQuery { get; set; } //to remove
        public string TestAssembliesPath { get; set; }
        public TimeSpan TestTimeout { get; set; }
        public int RetryAttempts { get; set; }
        public bool EnableBrowserExecutionInParallel { get; set; }
        public string[] SelectedTests { get; set; }
        public string EnvironmentUrl { get; set; }

        public bool ContainsTest(string methodName)
        {
            if (SelectedTests == null)
                return false;
            return SelectedTests.Contains(methodName);
        }
    }
}