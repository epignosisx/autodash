using System.Threading.Tasks;
using FluentValidation;
using MongoDB.Driver;
using System;

namespace Autodash.Core
{
    public class CreateSuiteCommand
    {
        private readonly IMongoDatabase _db;
        private readonly ITestAssembliesRepository _assembliesRepo;
        private readonly ILoggerWrapper _logger;

        public CreateSuiteCommand(IMongoDatabase db, ITestAssembliesRepository assembliesRepo, ILoggerProvider loggerProvider)
        {
            _db = db;
            _assembliesRepo = assembliesRepo;
            _logger = loggerProvider.GetLogger(GetType().Name);
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

            suite.Configuration.TestAssembliesPath = _assembliesRepo.MoveToTestSuite(suite, testAssembliesTempLocation);
            
            await coll.ReplaceOneAsync<TestSuite>(n => n.Id == suite.Id, suite);
            _logger.Info("Test Suite created {0} - {1}", suite.Id, suite.Name);

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