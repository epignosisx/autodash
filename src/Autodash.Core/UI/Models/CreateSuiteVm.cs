using System.Collections;
using System.Collections.Generic;
using FluentValidation.Results;
using System;

namespace Autodash.Core.UI.Models
{
    public class CreateSuiteVm
    {
        public string ProjectId { get; set; }
        public string SuiteName { get; set; }

        public string EnvironmentUrl { get; set; }
        public string[] Browsers { get; set; }
        public string TestAssembliesPath { get; set; }
        public int TestTimeoutMinutes { get; set; }
        public int RetryAttempts { get; set; }

        public TimeSpan? Time { get; set; }
        public int? IntervalHours { get; set; }

        public ValidationFailure[] Errors { get; set; }
        public List<KeyValuePair<string, string>> AvailableBrowsers { get; set; }

        public CreateSuiteVm()
        {
            RetryAttempts = 3;
            TestTimeoutMinutes = 10;
            Errors = new ValidationFailure[0];
        }

        public IEnumerable<Browser> GetBrowsers()
        {
            if(Browsers == null)
                yield break;

            foreach (var browser in Browsers)
            {
                var parts = browser.Split('|');
                yield return new Browser
                {
                    Name = parts[0],
                    Version = string.IsNullOrEmpty(parts[1]) ? null : parts[1]
                };
            }
        }
    }
}
