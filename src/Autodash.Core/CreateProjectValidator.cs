using FluentValidation;

namespace Autodash.Core
{
    public class CreateProjectValidator : AbstractValidator<Project>
    {
        public CreateProjectValidator()
        {
            RuleFor(p => p.Name).NotEmpty().Length(0, 100);
            RuleFor(p => p.Description).Length(0, 500);
            RuleFor(p => p.MemberEmails).SetCollectionValidator(new EmailValidator("MemberEmails"));
        }

        private class EmailValidator : AbstractValidator<string>
        {
            public EmailValidator(string collectionName)
            {
                RuleFor(x => x)
                    .NotEmpty()
                    .EmailAddress()
                    .OverridePropertyName(collectionName);
            }
        }
    }
}