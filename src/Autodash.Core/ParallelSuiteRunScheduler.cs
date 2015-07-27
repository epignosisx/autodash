using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Autodash.Core
{
    public class ParallelSuiteRunScheduler : ISuiteRunScheduler
    {
        private readonly Random _rand = new Random();
        private readonly ConcurrentQueue<SuiteRun> _onDemandQueue = new ConcurrentQueue<SuiteRun>();
        private readonly ConcurrentDictionary<string, Tuple<SuiteRun, CancellationTokenSource>> _runningSuites = new ConcurrentDictionary<string, Tuple<SuiteRun, CancellationTokenSource>>();
        private readonly ISuiteRunSchedulerRepository _repository;
        private readonly ISuiteRunner _suiteRunner;
        private readonly List<TestSuite> _scheduledSuites = new List<TestSuite>();
        private SeleniumGridConfiguration _gridConfig;
        private DateTime _lastSuiteRunDate;

        public ParallelSuiteRunScheduler(ISuiteRunSchedulerRepository repository, ISuiteRunner suiteRunner)
        {
            _repository = repository;
            _suiteRunner = suiteRunner;
        }

        public async Task Start()
        {
            await _repository.FailRunningSuitesAsync();

            List<TestSuite> scheduledSuites = await _repository.GetTestSuitesWithScheduleAsync();
            _scheduledSuites.AddRange(scheduledSuites);

            List<SuiteRun> runs = await _repository.GetScheduledSuiteRunsAsync();

            foreach (var run in runs)
                _onDemandQueue.Enqueue(run);

            //TODO: make robust
            Observable.Interval(TimeSpan.FromSeconds(10))
                .Do(async _ => await ReloadGridConfig())
                .Where(gridCon => _runningSuites.Count < _gridConfig.MaxParallelTestSuitesRunning)
                .Select(_ => NextSuiteToRun())
                .Where(suiteRun => suiteRun != null)
                .SelectMany(RunSuite)
                .SelectMany(SuiteCompleted)
                .Subscribe();
        }

        private async Task<SeleniumGridConfiguration> ReloadGridConfig()
        {
            if (_gridConfig == null || _rand.Next(100) < 20)
            {
                _gridConfig = await _repository.GetGridConfigurationAsync();
            }
            return _gridConfig;
        }

        private async Task<SuiteRun> RunSuite(SuiteRun suiteRun)
        {
            suiteRun.MarkAsRunning();
            await _repository.UpdateSuiteRunAsync(suiteRun);
            var cancellationSource = new CancellationTokenSource();
            _runningSuites.TryAdd(suiteRun.Id, Tuple.Create(suiteRun, cancellationSource));
            _lastSuiteRunDate = suiteRun.StartedOn;
            SuiteRun run = await _suiteRunner.Run(suiteRun, cancellationSource.Token);
            return run;
        }

        private async Task<SuiteRun> SuiteCompleted(SuiteRun suiteRun)
        {
            Tuple<SuiteRun, CancellationTokenSource> entry;
            if (_runningSuites.TryRemove(suiteRun.Id, out entry))
            {
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

        public async Task<SuiteRun> Schedule(TestSuite suite)
        {
            if (suite == null)
                throw new ArgumentNullException("suite");

            var run = SuiteRun.CreateSuiteRun(suite, DateTime.UtcNow);
            await _repository.AddSuiteRunAsync(run);
            _onDemandQueue.Enqueue(run);
            return run;
        }

        public bool TryCancelRunningSuite(string id)
        {
            Tuple<SuiteRun, CancellationTokenSource> entry;
            if (_runningSuites.TryRemove(id, out entry))
            {
                entry.Item2.Cancel();
                return true;
            }
            return false;
        }
    }
}