using FluentValidation.Results;
using System;

namespace Autodash.Core.UI.Models
{
    public class CreateSuiteVm
    {
        public string ProjectId { get; set; }
        public string SuiteName { get; set; }

        public string[] Browsers { get; set; }
        public string TestAssembliesPath { get; set; }
        public int TestTimeoutMinutes { get; set; }
        public int RetryAttempts { get; set; }

        public TimeSpan? Time { get; set; }
        public int? IntervalHours { get; set; }

        public ValidationFailure[] Errors { get; set; }

        public CreateSuiteVm()
        {
            RetryAttempts = 3;
            TestTimeoutMinutes = 10;
            Errors = new ValidationFailure[0];
        }
    }
}
