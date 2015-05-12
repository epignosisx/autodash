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
        private readonly IMongoDatabase _db;
        private readonly ISuiteRunner _suiteRunner;
        private readonly ConcurrentQueue<SuiteRun> _onDemandQueue = new ConcurrentQueue<SuiteRun>();
        private readonly List<TestSuite> _scheduledSuites = new List<TestSuite>();
        private Task<SuiteRun> _runningSuite;

        public DefaultSuiteRunScheduler(IMongoDatabase db, ISuiteRunner suiteRunner)
        {
            _db = db;
            _suiteRunner = suiteRunner;
        }

        public void Schedule(TestSuite suite)
        {
            if (suite == null)
                throw new ArgumentNullException("suite");
        }

        public async void Start()
        {
            var coll = _db.GetCollection<TestSuite>("TestSuite");
            var filter = new BsonDocument();//get all

            using (var cursor = await coll.FindAsync(filter))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var suite in batch)
                    {
                        if(suite.Schedule != null)
                            _scheduledSuites.Add(suite);
                    }
                }
            }

            var runColl = _db.GetCollection<SuiteRun>("SuiteRun");

            await FailRunningSuites(runColl);

            var runs = await runColl.Find(n => n.Status == SuiteRunStatus.Scheduled)
                .SortBy(n => n.ScheduledFor)
                .ToListAsync();

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

            if (nextSuite != null)
            {
                SuiteRun run = new SuiteRun
                {
                    StartedOn = now,
                    ScheduledFor = nextRunDate,
                    Status = SuiteRunStatus.Running,
                    TestSuiteId = nextSuite.Id,
                    TestSuiteSnapshot = nextSuite
                };

                _runningSuite =  _suiteRunner.Run(run);
                _runningSuite.ContinueWith(t => StartNextRun());
            }
        }

        private static async Task FailRunningSuites(IMongoCollection<SuiteRun> runColl)
        {
            var queryBuilder = Builders<SuiteRun>.Filter;
            var runningFilter = queryBuilder.Eq(n => n.Status, SuiteRunStatus.Running);

            var updateBuilder = Builders<SuiteRun>.Update;
            var updateDef = updateBuilder.Set(n => n.Status, SuiteRunStatus.Complete)
                .Set(n => n.CompletedOn, DateTime.UtcNow)
                .Set(n => n.Result, new SuiteRunResult("Did not complete", "Application stopped working. Running suites are stopped."));
            
            await runColl.UpdateManyAsync(runningFilter, updateDef);
        }
    }
}