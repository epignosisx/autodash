using FluentValidation;

namespace Autodash.Core
{
    public class UpdateProjectValidator : CreateProjectValidator
    {
        public UpdateProjectValidator()
        {
            RuleFor(p => p.Id).NotEmpty();
        }
    }
}