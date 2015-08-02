using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Autodash.Core
{
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
                .Set(n => n.Result, new SuiteRunResult("Did not complete. Application stopped working. Running suites were stopped."));

            await runColl.UpdateManyAsync(runningFilter, updateDef);
        }

        public async Task<List<TestSuite>> GetTestSuitesWithScheduleAsync()
        {
            var coll = _db.GetCollection<TestSuite>("TestSuite");
            var filter = new BsonDocument();//get all

            var suites = new List<TestSuite>();

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


        public Task<SeleniumGridConfiguration> GetGridConfigurationAsync()
        {
            var config = _db.GetCollection<SeleniumGridConfiguration>("SeleniumGridConfiguration")
                .FindAsync(new BsonDocument())
                .ToFirstOrDefaultAsync();

            return config;
        }
    }
}