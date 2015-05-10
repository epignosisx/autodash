using System;
using FluentValidation;

namespace Autodash.Core
{
    public class CreateTestSuiteValidator : AbstractValidator<TestSuite>
    {
        public CreateTestSuiteValidator()
        {
            RuleFor(p => p.Name).NotEmpty().Length(0, 100);
            RuleFor(p => p.ProjectId).NotEmpty();
            RuleFor(p => p.Configuration).NotNull();

            RuleFor(p => p.Configuration.Browsers).NotEmpty();
            RuleFor(p => p.Configuration.TestTagsQuery).Length(0, 500);

            RuleFor(p => p.Schedule.Time).InclusiveBetween(TimeSpan.Zero, new TimeSpan(23, 59, 59));
            RuleFor(p => p.Schedule.Interval).GreaterThanOrEqualTo(TimeSpan.FromMinutes(5));
        }
    }
}