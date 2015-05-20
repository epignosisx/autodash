using FluentValidation.Results;
using System;

namespace Autodash.Core.UI.Models
{
    public class CreateSuiteVm
    {
        public string ProjectId { get; set; }
        public string SuiteName { get; set; }

        public string[] Browsers { get; set; }
        public string TestTagsQuery { get; set; }
        public string TestAssembliesPath { get; set; }
        public TimeSpan TestTimeout { get; set; }
        public int RetryAttempts { get; set; }

        public TimeSpan Time { get; set; }
        public TimeSpan Interval { get; set; }

        public ValidationFailure[] Errors { get; set; }

        public CreateSuiteVm() 
        {
            Errors = new ValidationFailure[0];
        }
    }
}
