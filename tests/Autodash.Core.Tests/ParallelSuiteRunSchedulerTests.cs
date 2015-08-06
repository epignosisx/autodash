using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.Core;
using Xunit;

namespace Autodash.Core.Tests
{
    public class ParallelSuiteRunRunnerTests
    {
        private IUnitTestRunner Runner;

        private UnitTestCollection[] GetUnitTestCollections()
        {
            var test1 = new UnitTestInfo("Test1", null);
            var test2 = new UnitTestInfo("Test2", null);
            var unitTestCollection = new UnitTestCollection("TheAssemblyName.dll", null, new[] { test1, test2 }, Runner);
            return new UnitTestCollection[] { unitTestCollection };
        }

        private void CreateAlwaysFailRunnerMock(int millisecondsDelay)
        {
            Runner = Substitute.For<IUnitTestRunner>();
            Runner.Run(Arg.Any<TestRunContext>()).Returns(async (CallInfo ci)=> {
                await Task.Delay(millisecondsDelay);
                var browserNode = ci.Arg<TestRunContext>().GridNodeBrowserInfo;
                return new UnitTestBrowserResult
                {
                    Browser = new Browser
                    {
                        Name = browserNode.BrowserName,
                        Version = browserNode.Version
                    }
                };
            });
        }

        private void CreateAlwaysPassRunnerMock(int millisecondsDelay)
        {
            Runner = Substitute.For<IUnitTestRunner>();
            Runner.Run(Arg.Any<TestRunContext>()).Returns(async (CallInfo ci) =>
            {
                await Task.Delay(millisecondsDelay);
                var browserNode = ci.Arg<TestRunContext>().GridNodeBrowserInfo;
                return new UnitTestBrowserResult
                {
                    Passed = true,
                    Browser = new Browser
                    {
                        Name = browserNode.BrowserName,
                        Version = browserNode.Version
                    }
                };
            });
        }

        private SuiteRun GetSuiteRun()
        {
            return new SuiteRun
            {
                TestSuiteSnapshot = new TestSuite
                {
                    Configuration = new TestSuiteConfiguration
                    {
                        TestAssembliesPath = "c:\\",
                        EnvironmentUrl = "http://localhost",
                        Browsers = new[] { new Browser(BrowserNames.SeleniumFirefox), new Browser(BrowserNames.SeleniumChrome) },
                        RetryAttempts = 2,
                        SelectedTests = new[] { "Test1", "Test2" }
                    }
                }
            };
        }

        private SeleniumGridConfiguration GetGridConfig()
        {
            return new SeleniumGridConfiguration {HubUrl = "http://localhost", MaxParallelTestSuitesRunning = 1};
        }

        private List<GridNodeInfo> GetGridNodes()
        {
            var gridNodes = new List<GridNodeInfo>{
                new GridNodeInfo {
                    MaxSessions = 5,
                    Browsers = new List<GridNodeBrowserInfo>
                    {
                        new GridNodeBrowserInfo { BrowserName = "firefox", Protocol = "WebDriver", MaxInstances = 5},
                        new GridNodeBrowserInfo { BrowserName = "chrome", Protocol = "WebDriver", MaxInstances = 5}
                    },
                }
            };

            return gridNodes;
        }

        [Theory]
        [InlineData(0)]
        [InlineData(30)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(150)]
        [InlineData(200)]
        [InlineData(500)]
        [InlineData(600)]
        [InlineData(700)]
        //[InlineData(1000)]
        //[InlineData(5000)]
        //[InlineData(6000)]
        public async Task RunningSuiteWhereAllTestFail(int delay)
        {
            CreateAlwaysFailRunnerMock(delay);

            var discoverer = Substitute.For<ITestSuiteUnitTestDiscoverer>();
            discoverer.Discover(Arg.Any<string>()).Returns(GetUnitTestCollections());

            var scraper = Substitute.For<IGridConsoleScraper>();
            scraper.GetAvailableNodesInfoAsync(Arg.Any<Uri>()).Returns(Task.FromResult(GetGridNodes()));

            var repository = Substitute.For<ISuiteRunSchedulerRepository>();
            repository.GetScheduledSuiteRunsAsync().Returns(Task.FromResult(new List<SuiteRun>(0)));
            repository.GetTestSuitesWithScheduleAsync().Returns(Task.FromResult(new List<TestSuite>(0)));
            repository.GetGridConfigurationAsync().Returns(Task.FromResult(GetGridConfig()));

            var suiteRun = GetSuiteRun();

            var subject = new ParallelSuiteRunner(discoverer, scraper, repository, new FakeLoggerProvider());

            var result = await subject.Run(suiteRun, CancellationToken.None);

            subject.Dispose();

            Assert.NotNull(result);
            Assert.False(result.Result.Passed);
            Assert.NotEmpty(result.Result.Details);
            Assert.Equal(2, result.Result.FailedTotal);
            Assert.Equal(0, result.Result.PassedTotal);
            Assert.Equal(1, result.Result.CollectionResults.Count);
            Assert.NotEmpty(result.Result.CollectionResults[0].AssemblyName);
            Assert.Equal(2, result.Result.CollectionResults[0].UnitTestResults.Count);
            Assert.NotEmpty(result.Result.CollectionResults[0].UnitTestResults[0].TestName);
            Assert.NotEmpty(result.Result.CollectionResults[0].UnitTestResults[1].TestName);
            Assert.False(result.Result.CollectionResults[0].UnitTestResults[0].Passed);
            Assert.False(result.Result.CollectionResults[0].UnitTestResults[1].Passed);
            Assert.Equal(4, result.Result.CollectionResults[0].UnitTestResults[0].BrowserResults.Count);
            Assert.Equal(4, result.Result.CollectionResults[0].UnitTestResults[1].BrowserResults.Count);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(30)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(150)]
        [InlineData(200)]
        [InlineData(500)]
        [InlineData(600)]
        [InlineData(700)]
        //[InlineData(1000)]
        //[InlineData(5000)]
        //[InlineData(6000)]
        public async Task RunningSuiteWhereAllTestPass(int delay)
        {
            CreateAlwaysPassRunnerMock(delay);

            var discoverer = Substitute.For<ITestSuiteUnitTestDiscoverer>();
            discoverer.Discover(Arg.Any<string>()).Returns(GetUnitTestCollections());

            var scraper = Substitute.For<IGridConsoleScraper>();
            scraper.GetAvailableNodesInfoAsync(Arg.Any<Uri>()).Returns(Task.FromResult(GetGridNodes()));

            var repository = Substitute.For<ISuiteRunSchedulerRepository>();
            repository.GetScheduledSuiteRunsAsync().Returns(Task.FromResult(new List<SuiteRun>(0)));
            repository.GetTestSuitesWithScheduleAsync().Returns(Task.FromResult(new List<TestSuite>(0)));
            repository.GetGridConfigurationAsync().Returns(Task.FromResult(GetGridConfig()));

            var suiteRun = GetSuiteRun();

            var subject = new ParallelSuiteRunner(discoverer, scraper, repository, new FakeLoggerProvider());

            var result = await subject.Run(suiteRun, CancellationToken.None);

            subject.Dispose();

            Assert.NotNull(result);
            Assert.True(result.Result.Passed);
            Assert.NotEmpty(result.Result.Details);
            Assert.Equal(0, result.Result.FailedTotal);
            Assert.Equal(2, result.Result.PassedTotal);
            Assert.Equal(1, result.Result.CollectionResults.Count);
            Assert.NotEmpty(result.Result.CollectionResults[0].AssemblyName);
            Assert.Equal(2, result.Result.CollectionResults[0].UnitTestResults.Count);
            Assert.NotEmpty(result.Result.CollectionResults[0].UnitTestResults[0].TestName);
            Assert.NotEmpty(result.Result.CollectionResults[0].UnitTestResults[1].TestName);
            Assert.True(result.Result.CollectionResults[0].UnitTestResults[0].Passed);
            Assert.True(result.Result.CollectionResults[0].UnitTestResults[1].Passed);
            Assert.Equal(2, result.Result.CollectionResults[0].UnitTestResults[0].BrowserResults.Count);
            Assert.Equal(2, result.Result.CollectionResults[0].UnitTestResults[1].BrowserResults.Count);
        }
    }
}
