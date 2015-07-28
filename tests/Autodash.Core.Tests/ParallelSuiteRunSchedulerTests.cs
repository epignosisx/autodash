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
        private static IUnitTestRunner Runner;

        private static UnitTestCollection[] GetUnitTestCollections()
        {
            var test1 = new UnitTestInfo("Test1", null);
            var test2 = new UnitTestInfo("Test2", null);
            var unitTestCollection = new UnitTestCollection(null, null, new[] { test1, test2 }, Runner);
            return new UnitTestCollection[] { unitTestCollection };
        }

        private static void CreateRunnerMock()
        {
            Runner = Substitute.For<IUnitTestRunner>();
            Runner.Run(Arg.Any<TestRunContext>()).Returns(GetRunnerResult);
        }

        private static async Task<UnitTestBrowserResult> GetRunnerResult(CallInfo ci)
        {
            await Task.Delay(TimeSpan.FromSeconds(4));
            return new UnitTestBrowserResult
            {
                Browser = ci.Arg<TestRunContext>().GridNodeBrowserInfo.BrowserName
            };
        }

        private static SuiteRun GetSuiteRun()
        {
            return new SuiteRun
            {
                TestSuiteSnapshot = new TestSuite
                {
                    Configuration = new TestSuiteConfiguration
                    {
                        TestAssembliesPath = "c:\\",
                        EnvironmentUrl = "http://localhost",
                        Browsers = new[] { "firefox", "chrome" },
                        RetryAttempts = 2,
                        SelectedTests = new[] { "Test1", "Test2" }
                    }
                },
                Result = new SuiteRunResult
                {
                    CollectionResults = new UnitTestCollectionResult[]{
                        new UnitTestCollectionResult{
                            UnitTestResults = new List<UnitTestResult>{
                                new UnitTestResult { TestName = "Test1" },
                                new UnitTestResult { TestName = "Test2" }
                            }
                        }
                    }
                }
            };
        }

        private static SeleniumGridConfiguration GetGridConfig()
        {
            return new SeleniumGridConfiguration {HubUrl = "http://localhost", MaxParallelTestSuitesRunning = 1};
        }

        private static List<GridNodeInfo> GetGridNodes()
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


        [Fact]
        public async Task Foo()
        {
            CreateRunnerMock();

            var discoverer = Substitute.For<ITestSuiteUnitTestDiscoverer>();
            discoverer.Discover(Arg.Any<string>()).Returns(GetUnitTestCollections());

            var scraper = Substitute.For<IGridConsoleScraper>();
            scraper.GetAvailableNodesInfoAsync(Arg.Any<Uri>()).Returns(Task.FromResult(GetGridNodes()));

            var repository = Substitute.For<ISuiteRunSchedulerRepository>();
            repository.GetScheduledSuiteRunsAsync().Returns(Task.FromResult(new List<SuiteRun>(0)));
            repository.GetTestSuitesWithScheduleAsync().Returns(Task.FromResult(new List<TestSuite>(0)));
            repository.GetGridConfigurationAsync().Returns(Task.FromResult(GetGridConfig()));

            var suiteRun = GetSuiteRun();

            var subject = new ParallelSuiteRunner(discoverer, scraper, repository);

            var result = await subject.Run(suiteRun, CancellationToken.None);

            Assert.NotNull(result);
        }


    }

    public class ParallelSuiteRunSchedulerTests
    {

        [Fact]
        public void Foo()
        {
             var scraper = new DefaultGridConsoleScraper();
            //var obs = Observable.Create<GridNodeBrowserInfo>((observer) =>
            //{
            //    Timer timer = new Timer(async (state) =>
            //    {
            //        try
            //        {
            //            var nodes = await scraper.GetAvailableNodesInfoAsync(new Uri("http://alexappvm.cloudapp.net:4444/grid/console"));
            //            foreach (var node in nodes)
            //            {
            //                observer.OnNext(node);
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            observer.OnError(ex);
            //        }
            //    }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            //    return Disposable.Create(timer.Dispose);
            //});

            //var obs = Observable.Interval(TimeSpan.FromSeconds(5))
            //    .SkipWhile(n => )
            //    .SelectMany(n => scraper.GetAvailableNodesInfoAsync(new Uri("http://alexappvm.cloudapp.net:4444/grid/console")))
            //    .SelectMany(n => n);

            //obs.Subscribe(node =>
            //{
            //    var test = matcher.FindTest(node);
            //    var result = runner.Run(test);
            //});


            //Thread.Sleep(TimeSpan.FromSeconds(30));
        }
    }
}
