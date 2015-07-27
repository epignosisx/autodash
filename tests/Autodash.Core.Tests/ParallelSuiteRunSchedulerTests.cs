﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Xunit;
using Xunit.Abstractions;

namespace Autodash.Core.Tests
{
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

    public class ParallelSuiteRunner : ISuiteRunner
    {
        private readonly ITestSuiteUnitTestDiscoverer _unitTestDiscoverer;
        private readonly IGridConsoleScraper _gridConsoleScraper;
        private readonly ISuiteRunSchedulerRepository _repository;
        private readonly ConcurrentQueue<ParallelSuiteRunContext> _suiteRuns = new ConcurrentQueue<ParallelSuiteRunContext>();
        private Uri _hubUrl;

        private IDisposable _subscription;
        private int _isInitialized = 0;
        private int _processingNextTestsRound = 0;

        public ParallelSuiteRunner(
            ITestSuiteUnitTestDiscoverer unitTestDiscoverer, 
            IGridConsoleScraper gridConsoleScraper,
            ISuiteRunSchedulerRepository repository)
        {
            _unitTestDiscoverer = unitTestDiscoverer;
            _gridConsoleScraper = gridConsoleScraper;
            _repository = repository;
        }

        public async Task<SuiteRun> Run(SuiteRun run, CancellationToken cancellationToken)
        {
            if (run == null) 
                throw new ArgumentNullException("run");

            SuiteRunForRunnerValidator validator = new SuiteRunForRunnerValidator();
            ValidationResult validationResult = validator.Validate(run);
            if (!validationResult.IsValid)
            {
                var error = validationResult.Errors.First();
                run.Result = new SuiteRunResult(error.ErrorMessage);
                return run;
            }

            var testColls = _unitTestDiscoverer.Discover(run.TestSuiteSnapshot.Configuration.TestAssembliesPath).ToArray();
            if (testColls.Length == 0)
            {
                run.Result = new SuiteRunResult("There were not tests found in the test suite.");
                return run;
            }

            run.Result = new SuiteRunResult
            {
                CollectionResults = new UnitTestCollectionResult[testColls.Length]
            };

            var config = run.TestSuiteSnapshot.Configuration;

            for (int i = 0; i < testColls.Length; i++)
            {
                UnitTestCollection unitTestColl = testColls[i];
                UnitTestCollectionResult unitTestCollResult = new UnitTestCollectionResult
                {
                    AssemblyName = unitTestColl.AssemblyName,
                    UnitTestResults = new List<UnitTestResult>()
                };

                foreach (UnitTestInfo test in unitTestColl.Tests)
                {
                    bool shouldRun = config.SelectedTests == null || config.ContainsTest(test.TestName);
                    if (shouldRun)
                        unitTestCollResult.UnitTestResults.Add(new UnitTestResult(test.TestName));
                }

                run.Result.CollectionResults[i] = unitTestCollResult;
            }

            var gridConfig = await _repository.GetGridConfigurationAsync();
            _hubUrl = new Uri(gridConfig.HubUrl + "grid/console");

            var taskCompletionSource = new TaskCompletionSource<SuiteRun>();
            _suiteRuns.Enqueue(new ParallelSuiteRunContext(run, cancellationToken, testColls, taskCompletionSource));
            EnsureInitialized();
            return await taskCompletionSource.Task;
        }

        private void EnsureInitialized()
        {
            if (Interlocked.CompareExchange(ref _isInitialized, 1, 0) == 0)
            {
                _subscription = Observable.Interval(TimeSpan.FromSeconds(5))
                    .Where(n => _suiteRuns.Count > 0 && _processingNextTestsRound == 0)
                    .SelectMany(_ => _gridConsoleScraper.GetAvailableNodesInfoAsync(_hubUrl))
                    .Select(FindNextRuns)
                    .SelectMany(nextTests => nextTests)
                    .SelectMany(RunTestAsync)
                    .Subscribe();
            }
        }

        private async Task<UnitTestBrowserResult> RunTestAsync(Tuple<ParallelSuiteRunContext, TestRunContext> context)
        {
            var suiteRunContext = context.Item1;
            var testRunContext = context.Item2;
            var result = await testRunContext.UnitTestCollection.Runner.Run(testRunContext);

            var test = (from collResult in suiteRunContext.SuiteRun.Result.CollectionResults
                        from testResult in collResult.UnitTestResults
                        where collResult.AssemblyName == testRunContext.UnitTestCollection.AssemblyName
                            && testResult.TestName == testRunContext.UnitTestInfo.TestName
                        select testResult).First();
            
            lock (test.BrowserResults)
            {
                test.BrowserResults.Add(result);
            }
            return result;
        }

        private IEnumerable<Tuple<ParallelSuiteRunContext, TestRunContext>> FindNextRuns(List<GridNodeBrowserInfo> browserNodes)
        {
            ParallelSuiteRunContext firstSuiteDequeued = null;
            ParallelSuiteRunContext suiteRunContext;
            int prevBrowserNodesCount = browserNodes.Count;
            while (_suiteRuns.TryDequeue(out suiteRunContext))
            {
                if (firstSuiteDequeued == null)
                {
                    firstSuiteDequeued = suiteRunContext;
                }
                else if (firstSuiteDequeued == suiteRunContext && prevBrowserNodesCount > browserNodes.Count) 
                {
                    //we break when we have completed a full scan of the suites and no more
                    //tests can be run given the grid node browsers available.
                    yield break; 
                }

                if (suiteRunContext.CancellationToken.IsCancellationRequested)
                {
                    suiteRunContext.SuiteRun.Result.Details = "Suite Run was cancelled during execution.";
                    suiteRunContext.TaskCompletionSource.TrySetResult(suiteRunContext.SuiteRun);
                    
                    //reset if first suite
                    if (firstSuiteDequeued == suiteRunContext)
                        firstSuiteDequeued = null;
                }
                else
                {
                    _suiteRuns.Enqueue(suiteRunContext);// back to the end of the queue

                    Tuple<UnitTestCollection, UnitTestInfo, GridNodeBrowserInfo> nextTest =
                        suiteRunContext.FindNextTestToRun(browserNodes);

                    browserNodes.Remove(nextTest.Item3); //remove browser node so we do not double book a node.

                    if (nextTest != null)
                    {
                        var test = new TestRunContext(
                            nextTest.Item2,
                            nextTest.Item1,
                            suiteRunContext.SuiteRun.TestSuiteSnapshot.Configuration,
                            nextTest.Item3,
                            suiteRunContext.CancellationToken
                        );

                        yield return Tuple.Create(suiteRunContext, test);
                    }
                }
            }
        }
    }

    public class ParallelSuiteRunContext
    {
        public SuiteRun SuiteRun { get; private set; }
        public CancellationToken CancellationToken { get; private set; }
        public UnitTestCollection[] UnitTestCollections { get; private set; }
        public TaskCompletionSource<SuiteRun> TaskCompletionSource { get; private set; }

        public ParallelSuiteRunContext(SuiteRun suiteRun,
            CancellationToken cancellationToken,
            UnitTestCollection[] unitTestCollections,
            TaskCompletionSource<SuiteRun> taskCompletionSource)
        {
            SuiteRun = suiteRun;
            CancellationToken = cancellationToken;
            UnitTestCollections = unitTestCollections;
            TaskCompletionSource = taskCompletionSource;
        }

        public Tuple<UnitTestCollection, UnitTestInfo, GridNodeBrowserInfo> FindNextTestToRun(List<GridNodeBrowserInfo> browserNodes)
        {
            var suiteRun = SuiteRun;
            var config = suiteRun.TestSuiteSnapshot.Configuration;

            if (config.Browsers.All(b => browserNodes.All(bn => bn.BrowserName != b)))
                return null;

            for (int i = 0; i < suiteRun.Result.CollectionResults.Length; i++)
            {
                UnitTestCollectionResult collResult = suiteRun.Result.CollectionResults[i];
                var tests = collResult.GetPendingTestsToRun(config.Browsers, config.RetryAttempts);
                foreach (var test in tests)
                {
                    for (int j = 0; j < browserNodes.Count; j++)
                    {
                        var browserNode = browserNodes[j];
                        if (test.Browsers.Contains(browserNode.BrowserName))
                        {
                            return FindUnitTestCollection(test.UnitTestResult.TestName, browserNode);
                        }
                    }
                }
            }

            return null;
        }

        private Tuple<UnitTestCollection, UnitTestInfo, GridNodeBrowserInfo> FindUnitTestCollection(string testName, GridNodeBrowserInfo browserNode)
        {
            var q = from testColl in UnitTestCollections
                    from test in testColl.Tests
                    where test.TestName == testName
                    select Tuple.Create(testColl, test, browserNode);

            return q.First();
        }
    }

    public class ParallelSuiteRunScheduler : ISuiteRunScheduler
    {
        private readonly ConcurrentQueue<SuiteRun> _onDemandQueue = new ConcurrentQueue<SuiteRun>();
        private readonly List<Tuple<SuiteRun, CancellationTokenSource>> _runningSuites = new List<Tuple<SuiteRun, CancellationTokenSource>>();
        private readonly ISuiteRunSchedulerRepository _repository;
        private readonly ISuiteRunner _suiteRunner;
        private readonly List<TestSuite> _scheduledSuites = new List<TestSuite>();
        private readonly SeleniumGridConfiguration _gridConfig = new SeleniumGridConfiguration();
        private DateTime _lastSuiteRunDate;

        public ParallelSuiteRunScheduler(ISuiteRunSchedulerRepository repository, ISuiteRunner suiteRunner)
        {
            _repository = repository;
            _suiteRunner = suiteRunner;
            //_nextRunTimer = new Timer(NextSuiteRunCheck);
        }

        public async Task Start()
        {
            await _repository.FailRunningSuitesAsync();

            SeleniumGridConfiguration config = await _repository.GetGridConfigurationAsync();
            //_runningSuiteRuns.UpdateCapacity((config ?? new SeleniumGridConfiguration()).MaxParallelTestSuitesRunning);

            List<TestSuite> scheduledSuites = await _repository.GetTestSuitesWithScheduleAsync();
            _scheduledSuites.AddRange(scheduledSuites);

            List<SuiteRun> runs = await _repository.GetScheduledSuiteRunsAsync();

            foreach (var run in runs)
                _onDemandQueue.Enqueue(run);

            //TODO: make robust
            Observable.Interval(TimeSpan.FromSeconds(10))
                .SkipWhile(_ => _runningSuites.Count >= _gridConfig.MaxParallelTestSuitesRunning)
                .Select(_ => NextSuiteToRun())
                .SelectMany(RunSuite)
                .SelectMany(SuiteCompleted)
                .Subscribe();

        }

        private async Task<SuiteRun> RunSuite(SuiteRun suiteRun)
        {
            suiteRun.MarkAsRunning();
            await _repository.UpdateSuiteRunAsync(suiteRun);
            var cancellationSource = new CancellationTokenSource();
            _runningSuites.Add(Tuple.Create(suiteRun, cancellationSource));
            var run = await _suiteRunner.Run(suiteRun, cancellationSource.Token);
            return run;
        }

        private async Task<SuiteRun> SuiteCompleted(SuiteRun suiteRun)
        {
            Tuple<SuiteRun, CancellationTokenSource> entry = _runningSuites.FirstOrDefault(suite => suite.Item1 == suiteRun);

            if (entry != null)
            {
                _runningSuites.Remove(entry);
                suiteRun.MarkAsCompleted();
                await _repository.UpdateSuiteRunAsync(suiteRun);
            }
            return suiteRun;
        }

        private SuiteRun NextSuiteToRun()
        {
            var maxParallel = _gridConfig.MaxParallelTestSuitesRunning;
            if (_runningSuites.Count >= maxParallel)
                return null;

            SuiteRun onDemandRun;
            if (_onDemandQueue.TryDequeue(out onDemandRun))
                return onDemandRun;

            DateTime now = DateTime.UtcNow;
            DateTime lastRunDate = _lastSuiteRunDate == DateTime.MinValue ? now : _lastSuiteRunDate;
            DateTime nextRunDate = DateTime.MaxValue;
            var suites = _scheduledSuites.ToList();

            TestSuite nextSuite = null;
            foreach (var suite in suites)
            {
                var date = suite.Schedule.GetNextRunDate(lastRunDate);
                if (date < nextRunDate)
                {
                    nextRunDate = date;
                    nextSuite = suite;
                }
            }

            if (nextSuite != null)
                return SuiteRun.CreateSuiteRun(nextSuite, nextRunDate);

            return null;
        }

        public Task<SuiteRun> Schedule(TestSuite suite)
        {
            throw new NotImplementedException();
        }

        public bool TryCancelRunningSuite(string id)
        {
            throw new NotImplementedException();
        }
    }
}
