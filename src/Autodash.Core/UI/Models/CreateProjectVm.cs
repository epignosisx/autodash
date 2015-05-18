using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodash.Core.UI.Models
{
    public class CreateProjectVm
    {
        public string ProjectName { get; set; }
        public string Description { get; set; }

        public ValidationFailure[] Errors { get; set; }

        public CreateProjectVm()
        {
            Errors = new ValidationFailure[0];
        }
    }
}
