using FluentValidation;

namespace Autodash.Core
{
    public class UpdateEmailConfigValidator : AbstractValidator<EmailConfiguration>
    {
        public UpdateEmailConfigValidator()
        {
            RuleFor(p => p.FromEmail).NotEmpty().EmailAddress();
            RuleFor(p => p.Port).GreaterThan(0);
            RuleFor(p => p.SmtpServer).NotEmpty();
        }
    }
}