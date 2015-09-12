using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation;
using FluentValidation.Validators;

namespace Autodash.Core
{
    public class SuiteRunForRunnerValidator : AbstractValidator<SuiteRun>
    {
        public SuiteRunForRunnerValidator(IGridConsoleScraper gridScraper, Uri hubUri)
        {
            RuleFor(n => n.TestSuiteSnapshot).NotNull();
            RuleFor(n => n.TestSuiteSnapshot.Configuration).NotNull();
            RuleFor(n => n.TestSuiteSnapshot.Configuration.TestAssembliesPath).NotNull().SetValidator(new DirectoryExistsValidator());
            RuleFor(n => n.TestSuiteSnapshot.Configuration.Browsers).NotNull().SetValidator(new BrowserAvailabilityValidator(gridScraper, hubUri));
        }

        class BrowserAvailabilityValidator : PropertyValidator
        {
            private readonly IGridConsoleScraper _gridScraper;
            private readonly Uri _hubUri;

            public BrowserAvailabilityValidator(IGridConsoleScraper gridScraper, Uri hubUri)
                : base("Failed to Start. Grid does not support the following browsers: {Browsers}.")
            {
                _gridScraper = gridScraper;
                _hubUri = hubUri;
            }

            protected override bool IsValid(PropertyValidatorContext context)
            {
                var browsers = (Browser[]) context.PropertyValue;
                var nodes = _gridScraper.GetAvailableNodesInfo(_hubUri);

                List<Browser> unsatisfiedBrowsers = new List<Browser>(browsers.Length);
                foreach (var browser in browsers)
                {
                    bool satisfied = nodes.Any(node => node.Satisfies(browser, GridNodeBrowserInfo.WebDriverProtocol));
                    if (!satisfied)
                        unsatisfiedBrowsers.Add(browser);
                }

                if(unsatisfiedBrowsers.Count > 0)
                    context.MessageFormatter.AppendArgument("Browsers", string.Join(", ", unsatisfiedBrowsers));

                return unsatisfiedBrowsers.Count == 0;
            }
        }

        class DirectoryExistsValidator : PropertyValidator
        {
            public DirectoryExistsValidator()
                : base("Failed to Start. Test assemblies not found.")
            {
            }

            protected override bool IsValid(PropertyValidatorContext context)
            {
                var value = context.PropertyValue as string;
                if (string.IsNullOrEmpty(value))
                    return true;

                return Directory.Exists(value);
            }
        }
    }
}