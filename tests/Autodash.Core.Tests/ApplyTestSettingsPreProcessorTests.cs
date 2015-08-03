using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Autodash.Core.Tests
{
    public class ApplyTestSettingsPreProcessorTests
    {
        [Fact]
        public static void DoesNotThrow()
        {
            ApplyTestSettingsPreProcessor subject = new ApplyTestSettingsPreProcessor(new FakeLoggerProvider());
            var context = new TestRunnerPreProcessorContext
            {
                NodeBrowser = new GridNodeBrowserInfo
                {
                    BrowserName = "chrome"
                },
                GridConfiguration = new SeleniumGridConfiguration
                {
                    HubUrl = "http://localhost:4444"
                },
                TestDirectory = Environment.CurrentDirectory,
                TestSuiteConfiguration = new TestSuiteConfiguration
                {
                    EnvironmentUrl = "http://wwww.somedomain.com"
                }
            };

            subject.Process(context);

            Assert.True(true);
        }
    }
}
