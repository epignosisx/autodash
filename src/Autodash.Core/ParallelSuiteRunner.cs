using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;

namespace Autodash.Core
{
    public class ParallelSuiteRunner : ISuiteRunner, IDisposable
    {
        private readonly ITestSuiteUnitTestDiscoverer _unitTestDiscoverer;
        private readonly IGridConsoleScraper _gridConsoleScraper;
        private readonly ISuiteRunSchedulerRepository _repository;
        private readonly Queue<ParallelSuiteRunnerQueueItem> _testsQueue = new Queue<ParallelSuiteRunnerQueueItem>();
        private readonly HashSet<ParallelSuiteRunnerQueueItem> _runningTests = new HashSet<ParallelSuiteRunnerQueueItem>();
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

            var validator = new SuiteRunForRunnerValidator();
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

            var config = run.TestSuiteSnapshot.Configuration;
            var taskCompletionSource = new TaskCompletionSource<SuiteRun>();
            run.Result = new SuiteRunResult();

            lock (_testsQueue)
            {
                foreach (UnitTestCollection unitTestColl in testColls)
                {
                    foreach (UnitTestInfo test in unitTestColl.Tests)
                    {
                        bool shouldRun = config.SelectedTests == null || config.ContainsTest(test.TestName);
                        if (shouldRun)
                        {
                            foreach (var browser in config.Browsers)
                            {
                                _testsQueue.Enqueue(new ParallelSuiteRunnerQueueItem(browser, test, unitTestColl, run, taskCompletionSource, cancellationToken));
                            }
                        }
                    }
                }
            }

            var gridConfig = await _repository.GetGridConfigurationAsync();
            _hubUrl = new Uri(gridConfig.HubUrl + "grid/console");

            EnsureInitialized();
            return await taskCompletionSource.Task;
        }

        private void EnsureInitialized()
        {
            if (Interlocked.CompareExchange(ref _isInitialized, 1, 0) == 0)
            {
                _subscription = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(5))
                    .Where(_ => _testsQueue.Count > 0 && _processingNextTestsRound == 0)
                    .Do(_ => Interlocked.Exchange(ref _processingNextTestsRound, 1))
                    .SelectMany(_ => _gridConsoleScraper.GetAvailableNodesInfoAsync(_hubUrl))
                    .Select(nodes => new GridNodeManager(nodes))
                    .Select(FindNextTests)
                    .Do(_ => Interlocked.Exchange(ref _processingNextTestsRound, 0))
                    .SelectMany(tests => tests)
                    .SelectMany(RunTestAsync)
                    .Subscribe(test =>
                    {
                        Debug.WriteLine(test);
                    });
            }
        }

        private async Task<ParallelSuiteRunnerQueueItem> RunTestAsync(Tuple<ParallelSuiteRunnerQueueItem, GridNodeBrowserInfo> testInfo)
        {
            Debug.WriteLine("[{0:000}] RunTestAsync - Start: {1}", Thread.CurrentThread.ManagedThreadId, testInfo.Item1);
            var test = testInfo.Item1;
            var nodeBrowser = testInfo.Item2;
            
            var context = new TestRunContext(
                test.UnitTestInfo, 
                test.UnitTestCollection, 
                test.SuiteRun.TestSuiteSnapshot.Configuration, 
                nodeBrowser, 
                testInfo.Item1.CancellationToken
            );

            lock (_runningTests)
                _runningTests.Add(test);

            UnitTestBrowserResult result = await test.UnitTestCollection.Runner.Run(context);

            lock (_runningTests)
                _runningTests.Remove(test);

            HandleTestComplete(test, result);
            return test;
        }

        private void HandleTestComplete(ParallelSuiteRunnerQueueItem test, UnitTestBrowserResult browserResult)
        {
            lock (_testsQueue)
            {
                lock (test.SuiteRun.Result)
                {
                    //0. Check if cancellation was requested
                    if (test.CancellationToken.IsCancellationRequested)
                    {
                        test.SuiteRun.Result.Details = "Suite Run was cancelled during execution.";
                        test.TaskCompletionSource.TrySetResult(test.SuiteRun);
                        return;
                    }

                    //1. add result
                    var result = test.SuiteRun.Result;
                    var collResult = result.CollectionResults.FirstOrDefault(n => n.AssemblyName == test.UnitTestCollection.AssemblyName);
                    if (collResult == null)
                    {
                        collResult = new UnitTestCollectionResult { AssemblyName = test.UnitTestCollection.AssemblyName };
                        result.CollectionResults.Add(collResult);
                    }

                    var testResult = collResult.UnitTestResults.FirstOrDefault(n => n.TestName == test.UnitTestInfo.TestName);
                    if (testResult == null)
                    {
                        testResult = new UnitTestResult(test.UnitTestInfo.TestName);
                        collResult.UnitTestResults.Add(testResult);
                    }

                    testResult.BrowserResults.Add(browserResult);

                    //2. check if we the retry limit has been reached
                    var allBrowserResults = testResult.BrowserResults.Where(n => n.Browser == browserResult.Browser).ToList();
                    var allFailures = allBrowserResults.All(n => !n.Passed);
                    var maxRetryAttempts = test.SuiteRun.TestSuiteSnapshot.Configuration.RetryAttempts;
                    if (allFailures && allBrowserResults.Count < maxRetryAttempts)
                    {
                        _testsQueue.Enqueue(test);
                        return;
                    }

                    //3. check if suite is complete.
                    int pendingTests = _testsQueue.Count(n => n.SuiteRun == test.SuiteRun);
                    int runningTests = 0;
                    lock (_runningTests)
                        runningTests = _runningTests.Count(n => n.SuiteRun == test.SuiteRun);

                    if (pendingTests + runningTests == 0)
                    {
                        test.SuiteRun.Result.Details = "Ran to Completion";
                        test.TaskCompletionSource.TrySetResult(test.SuiteRun);
                    }
                }
            }
        }

        private IEnumerable<Tuple<ParallelSuiteRunnerQueueItem, GridNodeBrowserInfo>> FindNextTests(GridNodeManager gridNodeManager)
        {
            ParallelSuiteRunnerQueueItem firstTest = null;
            int prevBrowserNodesCount = gridNodeManager.GetAvailableBrowserNodes().Count();
            lock (_testsQueue)
            {
                while (_testsQueue.Count > 0)
                {
                    ParallelSuiteRunnerQueueItem test = _testsQueue.Dequeue();
                    if (test.CancellationToken.IsCancellationRequested)
                    {
                        HandleCancelledSuiteRun(test);
                    }
                    else
                    {
                        if (firstTest == null)
                        {
                            firstTest = test;
                        }
                        else if (firstTest == test && prevBrowserNodesCount >= gridNodeManager.GetAvailableBrowserNodes().Count())
                        {
                            //we break when we have completed a full scan of the tests and no other
                            //test can be run given a grid node browser given what is available.
                            _testsQueue.Enqueue(test);// back to the queue
                            yield break;
                        }

                        GridNodeBrowserInfo browserNode;
                        if (gridNodeManager.TryBook(test.Browser, out browserNode))
                        {
                            if (firstTest == test)
                                firstTest = null;

                            yield return Tuple.Create(test, browserNode);
                        }
                        else
                        {
                            _testsQueue.Enqueue(test);
                        }
                    }
                }
            }
        }

        private void HandleCancelledSuiteRun(ParallelSuiteRunnerQueueItem test)
        {
            //remove from queue other tests from the same suite
            ParallelSuiteRunnerQueueItem firstOtherTest = null;
            while (_testsQueue.Count > 0)
            {
                ParallelSuiteRunnerQueueItem otherTest = _testsQueue.Dequeue();
                if (otherTest.SuiteRun != test.SuiteRun)
                {
                    _testsQueue.Enqueue(otherTest);
                    if (firstOtherTest == null)
                        firstOtherTest = otherTest;
                    else if (firstOtherTest == otherTest)
                        break;
                }
            }

            lock (test.SuiteRun.Result)
            {
                test.SuiteRun.Result.Details = "Suite Run was cancelled during execution.";
            }

            test.TaskCompletionSource.TrySetResult(test.SuiteRun);
        }

        public void Dispose()
        {
            if (_subscription != null)
            {
                _subscription.Dispose();
                _subscription = null;
            }
        }
    }
    
    public class ParallelSuiteRunnerQueueItem
    {
        public ParallelSuiteRunnerQueueItem(
            string browser, 
            UnitTestInfo unitTestInfo, 
            UnitTestCollection unitTestCollection, 
            SuiteRun suiteRun,
            TaskCompletionSource<SuiteRun> taskCompletionSource,
            CancellationToken cancellationToken)
        {
            Browser = browser;
            UnitTestInfo = unitTestInfo;
            SuiteRun = suiteRun;
            TaskCompletionSource = taskCompletionSource;
            CancellationToken = cancellationToken;
            UnitTestCollection = unitTestCollection;
        }

        public string Browser { get; private set; }
        public UnitTestInfo UnitTestInfo { get; private set; }
        public UnitTestCollection UnitTestCollection { get; private set; }
        public SuiteRun SuiteRun { get; private set; }
        public TaskCompletionSource<SuiteRun> TaskCompletionSource { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}", UnitTestInfo.TestName, Browser);
        }
    }
}
