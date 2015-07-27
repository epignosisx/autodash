using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Autodash.Core
{
    public class DefaultSuiteRunScheduler : ISuiteRunScheduler
    {
        private readonly ISuiteRunSchedulerRepository _repository;
        private readonly ISuiteRunner _suiteRunner;
        private readonly ConcurrentQueue<SuiteRun> _onDemandQueue = new ConcurrentQueue<SuiteRun>();
        private readonly List<TestSuite> _scheduledSuites = new List<TestSuite>();
        private readonly Timer _nextRunTimer;
        private Task<SuiteRun> _runningSuiteTask;
        private SuiteRun _runningSuite;
        private CancellationTokenSource _runningSuiteCancelSource;
        private object _cancelSourceLock = new object();

        public DefaultSuiteRunScheduler(ISuiteRunSchedulerRepository repository, ISuiteRunner suiteRunner)
        {
            _repository = repository;
            _suiteRunner = suiteRunner;
            _nextRunTimer = new Timer(NextSuiteRunCheck);
        }

        public async Task<SuiteRun> Schedule(TestSuite suite)
        {
            if (suite == null)
                throw new ArgumentNullException("suite");

            var run = SuiteRun.CreateSuiteRun(suite, DateTime.UtcNow);
            await _repository.AddSuiteRunAsync(run);
            _onDemandQueue.Enqueue(run);
            return run;
        }

        public async Task Start()
        {
            List<TestSuite> scheduledSuites = await _repository.GetTestSuitesWithScheduleAsync();
            _scheduledSuites.AddRange(scheduledSuites);

            await _repository.FailRunningSuitesAsync();

            List<SuiteRun> runs = await _repository.GetScheduledSuiteRunsAsync();

            foreach (var run in runs)
                _onDemandQueue.Enqueue(run);
            
            await StartNextRun();

            _nextRunTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(10));
        }

        private async Task StartNextRun()
        {
            if (_runningSuiteTask != null && !_runningSuiteTask.IsCompleted)
                return;

            SuiteRun onDemandRun;
            if (_onDemandQueue.TryDequeue(out onDemandRun))
            {
                onDemandRun.StartedOn = DateTime.UtcNow;
                RunSuite(onDemandRun);
                return;
            }
            
            DateTime now = DateTime.UtcNow;
            DateTime lastRunDate = _runningSuiteTask == null ? DateTime.UtcNow : _runningSuiteTask.Result.StartedOn;
            TestSuite nextSuite = null;
            DateTime nextRunDate = DateTime.MaxValue;
            var suites = _scheduledSuites.ToList();
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
            {
                var run = SuiteRun.CreateSuiteRun(nextSuite, nextRunDate);
                run.StartedOn = now;

                await _repository.AddSuiteRunAsync(run);
                RunSuite(run);
            }
        }

        private void RunSuite(SuiteRun suiteRun)
        {
            suiteRun.Status = SuiteRunStatus.Running;
            _repository.UpdateSuiteRunAsync(suiteRun);
            _runningSuite = suiteRun;
            lock (_cancelSourceLock)
            {
                _runningSuiteCancelSource = new CancellationTokenSource();
            }
            _runningSuiteTask = _suiteRunner.Run(suiteRun, _runningSuiteCancelSource.Token);
            _runningSuiteTask.ContinueWith(SuiteRunCompleted);
        }

        private void SuiteRunCompleted(Task<SuiteRun> task)
        {
            try
            {
                var run = task.Result;
                run.CompletedOn = DateTime.UtcNow;
                run.Status = SuiteRunStatus.Complete;
                _runningSuite = null;
                _repository.UpdateSuiteRunAsync(run).Wait();
            }
            catch (Exception ex)
            {
                //TODO Log, in the meantime throw.
                throw new Exception();
            }
        }

        private void NextSuiteRunCheck(object state)
        {
            try
            {
                StartNextRun();
            }
            catch (Exception ex)
            {
                //TODO: log it
            }
        }

        public bool TryCancelRunningSuite(string id)
        {
            lock (_cancelSourceLock)
            {
                if (_runningSuiteCancelSource != null && 
                    _runningSuite != null &&
                    _runningSuite.Id == id)
                {
                    _runningSuiteCancelSource.Cancel();
                    return true;
                }    
            }
            return false;
        }
    }
}