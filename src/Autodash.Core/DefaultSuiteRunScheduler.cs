using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Autodash.Core
{
    public class DefaultSuiteRunScheduler : ISuiteRunScheduler
    {
        private readonly ISuiteRunSchedulerRepository _repository;
        private readonly ISuiteRunner _suiteRunner;
        private readonly ConcurrentQueue<SuiteRun> _onDemandQueue = new ConcurrentQueue<SuiteRun>();
        private readonly List<TestSuite> _scheduledSuites = new List<TestSuite>();
        private Task<SuiteRun> _runningSuite;

        public DefaultSuiteRunScheduler(ISuiteRunSchedulerRepository repository, ISuiteRunner suiteRunner)
        {
            _repository = repository;
            _suiteRunner = suiteRunner;
        }

        public void Schedule(TestSuite suite)
        {
            if (suite == null)
                throw new ArgumentNullException("suite");

        }

        public async Task Start()
        {
            List<TestSuite> scheduledSuites = await _repository.GetTestSuitesWithScheduleAsync();
            _scheduledSuites.AddRange(scheduledSuites);

            await _repository.FailRunningSuitesAsync();

            List<SuiteRun> runs = await _repository.GetScheduledSuiteRunsAsync();

            foreach (var run in runs)
                _onDemandQueue.Enqueue(run);
            
            StartNextRun();
        }

        private void StartNextRun()
        {
            SuiteRun onDemandRun;
            if (_onDemandQueue.TryDequeue(out onDemandRun))
            {
                _runningSuite = _suiteRunner.Run(onDemandRun);
                _runningSuite.ContinueWith(t => StartNextRun());
                return;
            }
            
            DateTime now = DateTime.UtcNow;
            DateTime lastRunDate = _runningSuite == null ? DateTime.UtcNow : _runningSuite.Result.StartedOn;
            TestSuite nextSuite = null;
            DateTime nextRunDate = DateTime.MaxValue;
            var suites = _scheduledSuites.ToList();
            foreach (var suite in suites)
            {
                var date = suite.Schedule.GetNextRunDate(now, lastRunDate);
                if (date < nextRunDate)
                {
                    nextRunDate = date;
                    nextSuite = suite;
                }
            }

            if (nextSuite != null && nextRunDate <= DateTime.UtcNow)//////what to do here?
            {
                SuiteRun run = new SuiteRun
                {
                    StartedOn = now,
                    ScheduledFor = nextRunDate,
                    Status = SuiteRunStatus.Running,
                    TestSuiteId = nextSuite.Id,
                    TestSuiteSnapshot = nextSuite
                };

                _runningSuite = _suiteRunner.Run(run);
                _runningSuite.ContinueWith(t => StartNextRun());
            }
        }

        
    }

    public interface ISuiteRunSchedulerRepository
    {
        Task FailRunningSuitesAsync();
        Task<List<TestSuite>> GetTestSuitesWithScheduleAsync();
        Task<List<SuiteRun>> GetScheduledSuiteRunsAsync();
    }

    public class SuiteRunSchedulerRepository : ISuiteRunSchedulerRepository
    {
        private readonly IMongoDatabase _db;

        public SuiteRunSchedulerRepository(IMongoDatabase database)
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
    }
}