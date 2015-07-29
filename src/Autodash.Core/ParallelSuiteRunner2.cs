using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;

namespace Autodash.Core
{
    public class ParallelSuiteRunner2 : ISuiteRunner
    {
        private readonly ITestSuiteUnitTestDiscoverer _unitTestDiscoverer;
        private readonly IGridConsoleScraper _gridConsoleScraper;
        private readonly ISuiteRunSchedulerRepository _repository;
        private readonly ConcurrentQueue<ParallelSuiteRunnerQueueItem> _testsQueue = new ConcurrentQueue<ParallelSuiteRunnerQueueItem>();
        private Uri _hubUrl;

        private IDisposable _subscription;
        private int _isInitialized = 0;
        private int _processingNextTestsRound = 0;

        public ParallelSuiteRunner2(
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

            for (int i = 0; i < testColls.Length; i++)
            {
                UnitTestCollection unitTestColl = testColls[i];

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
                    .Select(F)
                    .Subscribe();
            }
        }

        private IEnumerable<ParallelSuiteRunnerQueueItem> FindNextRuns(GridNodeManager gridNodeManager)
        {
            ParallelSuiteRunnerQueueItem test;
            while (_testsQueue.TryDequeue(out test))
            {
                if (test.CancellationToken.IsCancellationRequested)
                {
                    if (test.SuiteRun.Result == null)
                    {
                        test.SuiteRun.Result.Details
                    }
                }
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
    }
}
