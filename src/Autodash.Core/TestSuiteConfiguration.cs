using System;
namespace Autodash.Core
{
    public class TestSuiteConfiguration
    {
        public string[] Browsers { get; set; }
        public string TestTagsQuery { get; set; }
        public string TestAssembliesPath { get; set; }
        public TimeSpan TestTimeout { get; set; }
        public int RetryAttempts { get; set; }
    }
}