using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;

namespace Autodash.Core
{
    //public class ParallelSuiteRunner : ISuiteRunner
    //{
    //    private readonly ITestSuiteUnitTestDiscoverer _unitTestDiscoverer;
    //    private readonly IGridConsoleScraper _gridConsoleScraper;
    //    private readonly ISuiteRunSchedulerRepository _repository;
    //    private readonly ConcurrentQueue<ParallelSuiteRunContext> _suiteRuns = new ConcurrentQueue<ParallelSuiteRunContext>();
    //    private Uri _hubUrl;

    //    private IDisposable _subscription;
    //    private int _isInitialized = 0;
    //    private int _processingNextTestsRound = 0;

    //    public ParallelSuiteRunner(
    //        ITestSuiteUnitTestDiscoverer unitTestDiscoverer, 
    //        IGridConsoleScraper gridConsoleScraper,
    //        ISuiteRunSchedulerRepository repository)
    //    {
    //        _unitTestDiscoverer = unitTestDiscoverer;
    //        _gridConsoleScraper = gridConsoleScraper;
    //        _repository = repository;
    //    }

    //    public async Task<SuiteRun> Run(SuiteRun run, CancellationToken cancellationToken)
    //    {
    //        if (run == null) 
    //            throw new ArgumentNullException("run");

    //        var validator = new SuiteRunForRunnerValidator();
    //        ValidationResult validationResult = validator.Validate(run);
    //        if (!validationResult.IsValid)
    //        {
    //            var error = validationResult.Errors.First();
    //            run.Result = new SuiteRunResult(error.ErrorMessage);
    //            return run;
    //        }

    //        var testColls = _unitTestDiscoverer.Discover(run.TestSuiteSnapshot.Configuration.TestAssembliesPath).ToArray();
    //        if (testColls.Length == 0)
    //        {
    //            run.Result = new SuiteRunResult("There were not tests found in the test suite.");
    //            return run;
    //        }

    //        run.Result = new SuiteRunResult();

    //        var config = run.TestSuiteSnapshot.Configuration;

    //        for (int i = 0; i < testColls.Length; i++)
    //        {
    //            UnitTestCollection unitTestColl = testColls[i];
    //            var unitTestCollResult = new UnitTestCollectionResult
    //            {
    //                AssemblyName = unitTestColl.AssemblyName,
    //                UnitTestResults = new List<UnitTestResult>()
    //            };

    //            foreach (UnitTestInfo test in unitTestColl.Tests)
    //            {
    //                bool shouldRun = config.SelectedTests == null || config.ContainsTest(test.TestName);
    //                if (shouldRun)
    //                    unitTestCollResult.UnitTestResults.Add(new UnitTestResult(test.TestName));
    //            }

    //            run.Result.CollectionResults.Add(unitTestCollResult);
    //        }

    //        var gridConfig = await _repository.GetGridConfigurationAsync();
    //        _hubUrl = new Uri(gridConfig.HubUrl + "grid/console");

    //        var taskCompletionSource = new TaskCompletionSource<SuiteRun>();
    //        _suiteRuns.Enqueue(new ParallelSuiteRunContext(run, cancellationToken, testColls, taskCompletionSource));
    //        EnsureInitialized();
    //        return await taskCompletionSource.Task;
    //    }

    //    private void EnsureInitialized()
    //    {
    //        if (Interlocked.CompareExchange(ref _isInitialized, 1, 0) == 0)
    //        {
    //            _subscription = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(5))
    //                .Do(_ => ObsEx.DebugWriteLine(string.Format("New Tick. Processing?: {0}. Suite Run: {1}", _processingNextTestsRound, _suiteRuns.Count)))
    //                .Where(n => _suiteRuns.Count > 0 && _processingNextTestsRound == 0)
    //                .Do(_ => Interlocked.Exchange(ref _processingNextTestsRound, 1))
    //                .SelectMany(_ => _gridConsoleScraper.GetAvailableNodesInfoAsync(_hubUrl))
    //                .Select(nodes => new GridNodeManager(nodes))
    //                .Select(FindNextRuns)
    //                .Do(_ => Interlocked.Exchange(ref _processingNextTestsRound, 0))
    //                .SelectMany(nextTests => nextTests)
    //                .SelectMany(RunTestAsync)
    //                .Subscribe(
    //                    result => Debug.WriteLine("Runner Loop - Completed: " + result.TestName),
    //                    ex => Debug.WriteLine("Runner Loop - Error: " + ex.ToString())
    //                );
    //        }
    //    }

    //    private async Task<UnitTestResult> RunTestAsync(Tuple<ParallelSuiteRunContext, TestRunContext> context)
    //    {
    //        Debug.WriteLine("RunTestAsync - start: {0}", Thread.CurrentThread.ManagedThreadId);
    //        var suiteRunContext = context.Item1;
    //        var testRunContext = context.Item2;

    //        testRunContext.UnitTestResult.AddOngoingBrowserTest(testRunContext.GridNodeBrowserInfo.BrowserName);
    //        var result = await testRunContext.UnitTestCollection.Runner.Run(testRunContext);
    //        testRunContext.UnitTestResult.RemoveOngoingBrowserTest(testRunContext.GridNodeBrowserInfo.BrowserName);

    //        var test = (from collResult in suiteRunContext.SuiteRun.Result.CollectionResults
    //                    from testResult in collResult.UnitTestResults
    //                    where collResult.AssemblyName == testRunContext.UnitTestCollection.AssemblyName
    //                          && testResult.TestName == testRunContext.UnitTestInfo.TestName
    //                    select testResult).First();

    //        lock (test.BrowserResults)
    //        {
    //            test.BrowserResults.Add(result);
    //        }

    //        Debug.WriteLine("RunTestAsync - result: {0}", Thread.CurrentThread.ManagedThreadId);

    //        return test;
    //    }

    //    private IEnumerable<Tuple<ParallelSuiteRunContext, TestRunContext>> FindNextRuns(GridNodeManager gridNodeManager)
    //    {
    //        ParallelSuiteRunContext firstSuiteDequeued = null;
    //        ParallelSuiteRunContext suiteRunContext;
    //        int prevBrowserNodesCount = gridNodeManager.GetAvailableBrowserNodes().Count();
    //        while (_suiteRuns.TryDequeue(out suiteRunContext))
    //        {
    //            if (firstSuiteDequeued == null)
    //            {
    //                firstSuiteDequeued = suiteRunContext;
    //            }
    //            else if (firstSuiteDequeued == suiteRunContext && prevBrowserNodesCount >= gridNodeManager.GetAvailableBrowserNodes().Count()) 
    //            {
    //                //we break when we have completed a full scan of the suites and no more
    //                //tests can be run given the grid node browsers available.
    //                _suiteRuns.Enqueue(suiteRunContext);// back to the end of the queue
    //                yield break;
    //            }

    //            if (suiteRunContext.CancellationToken.IsCancellationRequested)
    //            {
    //                suiteRunContext.SuiteRun.Result.Details = "Suite Run was cancelled during execution.";
    //                suiteRunContext.TaskCompletionSource.TrySetResult(suiteRunContext.SuiteRun);
                    
    //                //reset if first suite
    //                if (firstSuiteDequeued == suiteRunContext)
    //                    firstSuiteDequeued = null;
    //            }
    //            else
    //            {
    //                _suiteRuns.Enqueue(suiteRunContext);// back to the end of the queue

    //                Tuple<UnitTestCollection, UnitTestInfo, UnitTestResult, GridNodeBrowserInfo> nextTest =
    //                    suiteRunContext.FindNextTestToRun(gridNodeManager);

    //                if (nextTest != null)
    //                {
    //                    gridNodeManager.Book(nextTest.Item4);

    //                    var test = new TestRunContext(
    //                        nextTest.Item2,
    //                        nextTest.Item1,
    //                        nextTest.Item3,
    //                        suiteRunContext.SuiteRun.TestSuiteSnapshot.Configuration,
    //                        nextTest.Item4,
    //                        suiteRunContext.CancellationToken
    //                        );

    //                    yield return Tuple.Create(suiteRunContext, test);
    //                }
    //            }
    //        }
    //    }
    //}
}