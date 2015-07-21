using FluentValidation.Results;

namespace Autodash.Core.UI.Models
{
    public class GridConfigVm
    {
        public string HubUrl { get; set; }

        public ValidationFailure[] Errors { get; set; }

        public GridConfigVm()
        {
            Errors = new ValidationFailure[0];
        }
    }
}