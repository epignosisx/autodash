using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Autodash.Core.Tests
{
    public class MsTestRunnerTests
    {
        [Fact]
        public async Task PassingTestExecutesAndPassedResultIsReturned()
        {
            MsTestRunner subject = new MsTestRunner();
            UnitTestInfo unitTest = new UnitTestInfo("Autodash.MsTest.ValidTests.UnitTest1.SuccessTest", null);
            UnitTestCollection coll = new UnitTestCollection("Autodash.MsTest.ValidTests", "Autodash.MsTest.ValidTests.dll", new []{unitTest}, subject);
            TestSuiteConfiguration config = new TestSuiteConfiguration{
                Browsers = new[] { new Browser{ Name = BrowserNames.SeleniumChrome} },
                TestAssembliesPath = Environment.CurrentDirectory
            };
            var browserNode = new GridNodeBrowserInfo { BrowserName = BrowserNames.SeleniumChrome };
            var context = new TestRunContext(unitTest, coll, config, browserNode, CancellationToken.None, GetGridConfig());

            UnitTestBrowserResult result = await subject.Run(context);

            Assert.Equal(result.Outcome, TestOutcome.Passed);
            Assert.False(Directory.Exists(Path.Combine(Environment.CurrentDirectory, "SuccessTest_chrome")));
        }

        [Fact]
        public async Task FailingTestExecutesAndFailedResultIsReturned()
        {
            MsTestRunner subject = new MsTestRunner();
            UnitTestInfo unitTest = new UnitTestInfo("Autodash.MsTest.ValidTests.UnitTest1.FailTest", null);
            UnitTestCollection coll = new UnitTestCollection("Autodash.MsTest.ValidTests", "Autodash.MsTest.ValidTests.dll", new[] { unitTest }, subject);
            TestSuiteConfiguration config = new TestSuiteConfiguration
            {
                Browsers = new[] { new Browser { Name = BrowserNames.SeleniumChrome } },
                TestAssembliesPath = Environment.CurrentDirectory
            };
            var browserNode = new GridNodeBrowserInfo { BrowserName = BrowserNames.SeleniumChrome };
            var context = new TestRunContext(unitTest, coll, config, browserNode, CancellationToken.None, GetGridConfig());
            UnitTestBrowserResult result = await subject.Run(context);

            Assert.Equal(result.Outcome, TestOutcome.Failed);
            Assert.False(Directory.Exists(Path.Combine(Environment.CurrentDirectory, "FailTest_chrome")));
        }

        [Fact]
        public async Task InconclusiveTestExecutesAndInconclusiveResultIsReturned()
        {
            MsTestRunner subject = new MsTestRunner();
            UnitTestInfo unitTest = new UnitTestInfo("Autodash.MsTest.ValidTests.UnitTest1.InconclusiveTest", null);
            UnitTestCollection coll = new UnitTestCollection("Autodash.MsTest.ValidTests", "Autodash.MsTest.ValidTests.dll", new[] { unitTest }, subject);
            TestSuiteConfiguration config = new TestSuiteConfiguration
            {
                Browsers = new[] { new Browser { Name = BrowserNames.SeleniumChrome } },
                TestAssembliesPath = Environment.CurrentDirectory
            };
            var browserNode = new GridNodeBrowserInfo { BrowserName = BrowserNames.SeleniumChrome };
            var context = new TestRunContext(unitTest, coll, config, browserNode, CancellationToken.None, GetGridConfig());
            UnitTestBrowserResult result = await subject.Run(context);

            Assert.Equal(result.Outcome, TestOutcome.Inconclusive);
            Assert.False(Directory.Exists(Path.Combine(Environment.CurrentDirectory, "InconclusiveTest_chrome")));
        }

        [Fact]
        public async Task TestThatTimesOutThrows()
        {
            MsTestRunner subject = new MsTestRunner();
            UnitTestInfo unitTest = new UnitTestInfo("Autodash.MsTest.ValidTests.UnitTest1.AnotherSuccessTest", null);
            UnitTestCollection coll = new UnitTestCollection("Autodash.MsTest.ValidTests", "Autodash.MsTest.ValidTests.dll", new[] { unitTest }, subject);
            TestSuiteConfiguration config = new TestSuiteConfiguration
            {
                Browsers = new[] { new Browser { Name = BrowserNames.SeleniumChrome } },
                TestAssembliesPath = Environment.CurrentDirectory,
                TestTimeout = TimeSpan.FromSeconds(2)
            };

            var browserNode = new GridNodeBrowserInfo { BrowserName = BrowserNames.SeleniumChrome };
            var context = new TestRunContext(unitTest, coll, config, browserNode, CancellationToken.None, GetGridConfig());
            UnitTestBrowserResult result = await subject.Run(context);
            Assert.Equal(result.Outcome, TestOutcome.Failed);
            Assert.Equal(result.Stdout, "Test timed out");
        }

        private static SeleniumGridConfiguration GetGridConfig()
        {
            return new SeleniumGridConfiguration {HubUrl = "http://localhost:4444", Id = "1", MaxParallelTestSuitesRunning = 5};
        }
    }
}
