using FluentValidation;

namespace Autodash.Core
{
    public class CreateProjectValidator : AbstractValidator<Project>
    {
        public CreateProjectValidator()
        {
            RuleFor(p => p.Name).NotEmpty().Length(0, 100);
            RuleFor(p => p.Description).Length(0, 500);
        }
    }

}