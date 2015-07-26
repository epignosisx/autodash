using System.IO;
using FluentValidation;
using FluentValidation.Validators;

namespace Autodash.Core
{
    public class SuiteRunForRunnerValidator : AbstractValidator<SuiteRun>
    {
        public SuiteRunForRunnerValidator()
        {
            RuleFor(n => n.TestSuiteSnapshot).NotNull();
            RuleFor(n => n.TestSuiteSnapshot.Configuration).NotNull();
            RuleFor(n => n.TestSuiteSnapshot.Configuration.TestAssembliesPath).NotNull().SetValidator(new DirectoryExistsValidator());
        }

        class DirectoryExistsValidator : PropertyValidator
        {
            public DirectoryExistsValidator()
                : base("Property {PropertyName} is not a valid directory.")
            {
            }

            protected override bool IsValid(PropertyValidatorContext context)
            {
                var value = context.PropertyValue as string;
                if (string.IsNullOrEmpty(value))
                    return true;

                var exists = Directory.Exists(value);
                if (!exists)
                    context.MessageFormatter.BuildMessage("Failed to Start. Test assemblies not found at: " + value);
                return exists;
            }
        }
    }
}