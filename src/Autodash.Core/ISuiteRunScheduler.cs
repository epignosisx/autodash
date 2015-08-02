using System;
using System.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
namespace Autodash.Core
{
    public interface ISuiteRunScheduler
    {
        Task Start();
        Task<SuiteRun> Schedule(TestSuite suite);
        bool TryCancelRunningSuite(string id);
    }

    //public class ParallelSuiteRunScheduler : ISuiteRunScheduler
    //{
    //    private readonly ISuiteRunSchedulerRepository _repository;
    //    private readonly ConcurrentQueue<SuiteRun> _onDemandQueue = new ConcurrentQueue<SuiteRun>();
    //    private readonly BoundedUpdatableCollection<RunnerSuiteRunContext> _runningSuiteRuns = new BoundedUpdatableCollection<RunnerSuiteRunContext>();

    //    private readonly List<TestSuite> _scheduledSuites = new List<TestSuite>();
    //    private readonly Timer _nextRunTimer;

    //    private DateTime _lastSuiteRunDate;

    //    public ParallelSuiteRunScheduler(ISuiteRunSchedulerRepository repository)
    //    {
    //        _repository = repository;
    //        _nextRunTimer = new Timer(NextSuiteRunCheck);
    //    }

    //    public async Task Start()
    //    {
    //        var config = await _repository.GetGridConfigurationAsync();
    //        _runningSuiteRuns.UpdateCapacity((config ?? new SeleniumGridConfiguration()).MaxParallelTestSuitesRunning);
            
    //        List<TestSuite> scheduledSuites = await _repository.GetTestSuitesWithScheduleAsync();
    //        _scheduledSuites.AddRange(scheduledSuites);

    //        await _repository.FailRunningSuitesAsync();

    //        List<SuiteRun> runs = await _repository.GetScheduledSuiteRunsAsync();

    //        foreach (var run in runs)
    //            _onDemandQueue.Enqueue(run);

    //        await StartNextRun();

    //        _nextRunTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(10));
    //    }

    //    private async Task EnsureRunningQueueHasCapacity()
    //    {
    //        var config = await _repository.GetGridConfigurationAsync();
    //        if(_runningSuiteRuns == null)
    //            _runningSuiteRuns.                
    //        if (config == null)
    //            return 1;
    //        return config.MaxParallelTestSuitesRunning;
    //    }

    //    private async Task StartNextRun()
    //    {
    //        if (_runningSuiteRuns.IsFull)
    //            return;

    //        SuiteRun onDemandRun;
    //        if (_onDemandQueue.TryDequeue(out onDemandRun))
    //        {
    //            onDemandRun.StartedOn = DateTime.UtcNow;
    //            RunSuite(onDemandRun);
    //            return;
    //        }

    //        DateTime now = DateTime.UtcNow;
    //        DateTime lastRunDate = _lastSuiteRunDate == DateTime.MinValue ? now : _lastSuiteRunDate;
    //        TestSuite nextSuite = null;
    //        DateTime nextRunDate = DateTime.MaxValue;
    //        var suites = _scheduledSuites.ToList();
    //        foreach (var suite in suites)
    //        {
    //            var date = suite.Schedule.GetNextRunDate(lastRunDate);
    //            if (date < nextRunDate)
    //            {
    //                nextRunDate = date;
    //                nextSuite = suite;
    //            }
    //        }

    //        if (nextSuite != null)
    //        {
    //            var run = SuiteRun.CreateSuiteRun(nextSuite, nextRunDate);
    //            run.StartedOn = now;
    //            _lastSuiteRunDate = now;

    //            await _repository.AddSuiteRunAsync(run);
    //            RunSuite(run);
    //        }
    //    }

    //    private void RunSuite(SuiteRun suiteRun)
    //    {
    //        suiteRun.Status = SuiteRunStatus.Running;
    //        _repository.UpdateSuiteRunAsync(suiteRun);
    //        _runningSuite = suiteRun;
    //        lock (_cancelSourceLock)
    //        {
    //            _runningSuiteCancelSource = new CancellationTokenSource();
    //        }
    //        _runningSuiteTask = _suiteRunner.Run(suiteRun, _runningSuiteCancelSource.Token);
    //        _runningSuiteTask.ContinueWith(SuiteRunCompleted);
    //    }

    //    private void NextSuiteRunCheck(object state)
    //    {
    //        try
    //        {
    //            StartNextRun();
    //        }
    //        catch (Exception ex)
    //        {
    //            //TODO: log it
    //        }
    //    }

    //    public Task<SuiteRun> Schedule(TestSuite suite)
    //    {
    //        throw new System.NotImplementedException();
    //    }

    //    public bool TryCancelRunningSuite(string id)
    //    {
    //        throw new System.NotImplementedException();
    //    }
    //}

    //public class RunnerSuiteRunContext
    //{
    //    private readonly SuiteRun _run;
    //    private readonly CancellationToken _cancellationToken;

    //    public RunnerSuiteRunContext(SuiteRun run, CancellationToken cancellationToken)
    //    {
    //        if (run == null) 
    //            throw new ArgumentNullException("run");
            
    //        _run = run;
    //        _cancellationToken = cancellationToken;
    //    }
    //}
}