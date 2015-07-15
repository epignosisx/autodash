using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
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

        public SuiteRun GetRunningSuite()
        {
            return _runningSuite;
        }

        public void CancelRunningSuite()
        {
            lock (_cancelSourceLock)
            {
                if (_runningSuiteCancelSource != null)
                    _runningSuiteCancelSource.Cancel();    
            }
        }
    }

    public interface ISuiteRunSchedulerRepository
    {
        Task FailRunningSuitesAsync();
        Task<List<TestSuite>> GetTestSuitesWithScheduleAsync();
        Task<List<SuiteRun>> GetScheduledSuiteRunsAsync();
        Task AddSuiteRunAsync(SuiteRun run);
        Task UpdateSuiteRunAsync(SuiteRun run);
    }

    public class DefaultSuiteRunSchedulerRepository : ISuiteRunSchedulerRepository
    {
        private readonly IMongoDatabase _db;

        public DefaultSuiteRunSchedulerRepository(IMongoDatabase database)
        {
            _db = database;
        }

        public async Task FailRunningSuitesAsync()
        {
            var runColl = _db.GetCollection<SuiteRun>("SuiteRun");
            var queryBuilder = Builders<SuiteRun>.Filter;
            var runningFilter = queryBuilder.Eq(n => n.Status, SuiteRunStatus.Running);

            var updateBuilder = Builders<SuiteRun>.Update;
            var updateDef = updateBuilder.Set(n => n.Status, SuiteRunStatus.Complete)
                .Set(n => n.CompletedOn, DateTime.UtcNow)
                .Set(n => n.Result, new SuiteRunResult("Did not complete", "Application stopped working. Running suites are stopped."));

            await runColl.UpdateManyAsync(runningFilter, updateDef);
        }

        public async Task<List<TestSuite>> GetTestSuitesWithScheduleAsync()
        {
            var coll = _db.GetCollection<TestSuite>("TestSuite");
            var filter = new BsonDocument();//get all

            List<TestSuite> suites = new List<TestSuite>();

            using (var cursor = await coll.FindAsync(filter))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var suite in batch)
                    {
                        if (suite.Schedule != null)
                            suites.Add(suite);
                    }
                }
            }

            return suites;
        }

        public Task<List<SuiteRun>> GetScheduledSuiteRunsAsync()
        {
            var runColl = _db.GetCollection<SuiteRun>("SuiteRun");

            return runColl.Find(n => n.Status == SuiteRunStatus.Scheduled)
                .SortBy(n => n.ScheduledFor)
                .ToListAsync();
        }

        public Task AddSuiteRunAsync(SuiteRun run)
        {
            var runColl = _db.GetCollection<SuiteRun>("SuiteRun");
            return runColl.InsertOneAsync(run);
        }

        public Task UpdateSuiteRunAsync(SuiteRun run)
        {
            var runColl = _db.GetCollection<SuiteRun>("SuiteRun");

            var queryBuilder = Builders<SuiteRun>.Filter;
            var filterById = queryBuilder.Eq(n => n.Id, run.Id);

            return runColl.ReplaceOneAsync(filterById, run);
        }
    }
}