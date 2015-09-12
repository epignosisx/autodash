using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly ILoggerWrapper _logger;
        private readonly List<TestSuite> _scheduledSuites = new List<TestSuite>();
        private SeleniumGridConfiguration _gridConfig;
        private DateTime _lastSuiteRunDate;

        public ParallelSuiteRunScheduler(ISuiteRunSchedulerRepository repository, ISuiteRunner suiteRunner, ILoggerProvider loggerProvider)
        {
            _repository = repository;
            _suiteRunner = suiteRunner;
            _logger = loggerProvider.GetLogger(GetType().Name);
        }

        public async Task Start()
        {
            _logger.Info("Scheduler started");

            await _repository.FailRunningSuitesAsync();

            List<TestSuite> scheduledSuites = await _repository.GetTestSuitesWithScheduleAsync();
            _scheduledSuites.AddRange(scheduledSuites);

            List<SuiteRun> runs = await _repository.GetScheduledSuiteRunsAsync();

            foreach (var run in runs)
                _onDemandQueue.Enqueue(run);

            StartTimer();
        }

        private void StartTimer()
        {
            Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(10))
                .Do(async _ => await ReloadGridConfig())
                .Where(n => _gridConfig != null)
                .Where(gridCon => _runningSuites.Count < _gridConfig.MaxParallelTestSuitesRunning)
                .Select(_ => NextSuiteToRun())
                .Where(suiteRun => suiteRun != null)
                .SelectMany(RunSuite)
                .SelectMany(SuiteCompleted)
                .RetryWithBackoffStrategy(50)
                .Subscribe(n => 
                    _logger.Info("Suite Run {0} - {1} completed.", n.Id, n.TestSuiteSnapshot.Name), 
                    ex => _logger.Error(ex, "Scheduler timer failed")
                );
        }

        private async Task<SeleniumGridConfiguration> ReloadGridConfig()
        {
            if (_gridConfig == null || _rand.Next(100) < 20)
            {
                _gridConfig = await _repository.GetGridConfigurationAsync();
                _logger.Info("Grid Config reloaded");
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

            _logger.Info("Starting Suite Run {0} - {1}", suiteRun.Id, suiteRun.TestSuiteSnapshot.Name);
            SuiteRun run = await _suiteRunner.Run(suiteRun, cancellationSource.Token);
            _logger.Info("Ended Suite Run {0} - {1}", suiteRun.Id, suiteRun.TestSuiteSnapshot.Name);

            return run;
        }

        private async Task<SuiteRun> SuiteCompleted(SuiteRun suiteRun)
        {
            suiteRun.MarkAsCompleted();
            await _repository.UpdateSuiteRunAsync(suiteRun);
            Tuple<SuiteRun, CancellationTokenSource> ignore;
            _runningSuites.TryRemove(suiteRun.Id, out ignore);
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

            _logger.Info("Scheduled Suite Run {0} - {1}", run.Id, run.TestSuiteSnapshot.Name);
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