using FluentValidation.Results;
using System;

namespace Autodash.Core.UI.Models
{
    public class CreateSuiteVm
    {
        public string ProjectId { get; set; }
        public string SuiteName { get; set; }

        public ValidationFailure[] Errors { get; set; }

        public CreateSuiteVm() 
        {
            Errors = new ValidationFailure[0];
        }
    }
}
