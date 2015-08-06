using System;
using System.Linq;

namespace Autodash.Core
{
    public class TestSuiteConfiguration
    {
        public Browser[] Browsers { get; set; }
        public string TestAssembliesPath { get; set; }
        public TimeSpan TestTimeout { get; set; }
        public int RetryAttempts { get; set; }
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