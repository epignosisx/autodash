using System.Threading.Tasks;
using FluentValidation;
using MongoDB.Driver;
using System;

namespace Autodash.Core
{
    public class CreateSuiteCommand
    {
        private IMongoDatabase _db;
        private ITestAssembliesRepository _assembliesRepo;
        
        public CreateSuiteCommand(IMongoDatabase db, ITestAssembliesRepository assembliesRepo)
        {
            _db = db;
            _assembliesRepo = assembliesRepo;
        }

        public async Task ExecuteAsync(TestSuite suite, string testAssembliesTempLocation)
        {
            if (suite == null)
                throw new ArgumentNullException("suite");
            if (testAssembliesTempLocation == null)
                throw new ArgumentNullException("testAssembliesTempLocation");

            var validator = new CreateTestSuiteValidator();
            validator.ValidateAndThrow(suite);

            var coll = _db.GetCollection<TestSuite>("TestSuite");
            await coll.InsertOneAsync(suite);

            _assembliesRepo.MoveToTestSuite(suite, testAssembliesTempLocation);

            await coll.ReplaceOneAsync<TestSuite>(n => n.Id == suite.Id, suite);
        }
    }

    public class ScheduleOnDemandTestSuiteCommand
    {
        private readonly ISuiteRunScheduler _scheduler;

        public ScheduleOnDemandTestSuiteCommand(ISuiteRunScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public void Execute(TestSuite suite)
        {
            if(suite == null)
                throw new ArgumentNullException("suite");

            _scheduler.Schedule(suite);
        }
    }
}