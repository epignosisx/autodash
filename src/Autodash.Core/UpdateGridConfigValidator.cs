using System;
using FluentValidation;
using FluentValidation.Validators;

namespace Autodash.Core
{
    public class UpdateGridConfigValidator : AbstractValidator<SeleniumGridConfiguration>
    {
        public UpdateGridConfigValidator()
        {
            RuleFor(p => p.HubUrl).NotEmpty().SetValidator(new StringMustBeRootUrlValidator());
            RuleFor(p => p.MaxParallelTestSuitesRunning).InclusiveBetween(0, 5);
        }

        class StringMustBeRootUrlValidator : PropertyValidator
        {
            public StringMustBeRootUrlValidator() : base("Property {PropertyName} is not a root url.")
            {
            }

            protected override bool IsValid(PropertyValidatorContext context)
            {
                var url = context.PropertyValue as string;
                if (string.IsNullOrEmpty(url))
                    return true;

                Uri uri;
                if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                {
                    return uri.AbsolutePath == "/";
                }
                return false;
            }
        }
    }
}