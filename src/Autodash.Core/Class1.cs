using FluentValidation;
using FluentValidation.Results;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodash.Core
{
    public class TestSuite
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public TestSuiteConfiguration Configuration { get; set; }
        public TestSuiteSchedule Schedule { get; set; }
    }

    public class TestSuiteConfiguration
    {
        public string[] Browsers { get; set; }
        public string TestCategoriesQuery { get; set; }
        public string TestAssembliesPath { get; set; }
    }

    public class TestSuiteSchedule
    {
        public TimeSpan Time { get; set; }
        public TimeSpan Interval { get; set; }

        public override string ToString()
        {
            return string.Format("Runs at {0} every {1}", Time, Interval);
        }
    }

    public enum SuiteRunStatus{
        Scheduled,
        Running,
        Complete
    }

    public class SuiteRun
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public DateTime ScheduledFor { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime CompletedOn { get; set; }
        public SuiteRunStatus Status { get; set; }
        public string TestSuiteId { get; set; }
        public SuiteRunResult Result { get; set; }
    }

    public class SuiteRunResult
    {
        public SuiteRunResultStatus Status { get; set; }
    }

    public enum SuiteRunResultStatus
    {
        RanToCompletion,
        Cancelled,
        UnexpectedFailure
    }

    public class DefaultSuiteRunScheduler : ISuiteRunScheduler
    {
        public void Start()
        {
 	        throw new NotImplementedException();
        }

        public void Schedule(SuiteRun run)
        {
 	        throw new NotImplementedException();
        }
    }

    public interface ISuiteRunScheduler
    {
        void Start();
        void Schedule(SuiteRun run);
    }

    public class DefaultSuiteRunScheduler : ISuiteRunScheduler
    {
        private readonly IMongoDatabase _db;
        private readonly SortedList<DateTime, TestSuite> _runsQueue;

        public DefaultSuiteRunScheduler(IMongoDatabase db)
        {
            _db = db;
        }

        public async Task Schedule(TestSuite suite)
        {
            if (suite == null)
                throw new ArgumentNullException("suite");

            _suitesQueue.Add(suite);

            
        }

        public async void Start()
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
                        suites.Add(suite);
                    }
                }
            }

            var runColl = _db.GetCollection<SuiteRun>("SuiteRun");

            await FailRunningSuites(runColl);

            _runsQueue.Clear();
            foreach (var suite in suites)
            {
                var runs = await runColl.Find(n => n.Status == SuiteRunStatus.Scheduled && n.TestSuiteId == suite.Id)
                    .SortBy(n => n.ScheduledFor)
                    .ToListAsync();

                foreach(var run in runs)
                    _runsQueue.Add(run.ScheduledFor, run);
            }
            
        }

        private static async Task FailRunningSuites(IMongoCollection<SuiteRun> runColl)
        {
            var queryBuilder = Builders<SuiteRun>.Filter;
            var runningFilter = queryBuilder.Eq(n => n.Status, SuiteRunStatus.Running);

            var updateBuilder = Builders<SuiteRun>.Update;
            var updateDef = updateBuilder.Set(n => n.Status, SuiteRunStatus.Complete)
                                      .Set(n => n.CompletedOn, DateTime.UtcNow)
                                      .Set(n => n.Result, new SuiteRunResult { Status = SuiteRunResultStatus.UnexpectedFailure });

            await runColl.UpdateManyAsync(runningFilter, updateDef);
        }

        private static SuiteRun GetNextRun()
        {

        }
    }

    public class Project
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }

    public class CreateProjectCommand
    {
        public Task ExecuteAsync(IMongoDatabase db, Project project)
        {
            var validator = new CreateProjectValidator();
            validator.ValidateAndThrow(project);

            var coll = db.GetCollection<Project>("Project");
            return coll.InsertOneAsync(project);
        }
    }

    public class CreateSuiteCommand
    {
        public async Task ExecuteAsync(IMongoDatabase db, ITestAssembliesRepository assembliesRepo, TestSuite suite, string testAssembliesTempLocation)
        {
            var validator = new CreateTestSuiteValidator();
            validator.ValidateAndThrow(suite);

            var coll = db.GetCollection<TestSuite>("TestSuite");
            await coll.InsertOneAsync(suite);

            assembliesRepo.MoveToTestSuite(suite, testAssembliesTempLocation);

            await coll.ReplaceOneAsync<TestSuite>(n => n.Id == suite.Id, suite);
        }
    }

    public interface ITestAssembliesRepository
    {
        void MoveToTestSuite(TestSuite suite, string currentLocation);
    }

    public class FileSystemTestAssembliesRepository : ITestAssembliesRepository
    {
        private readonly string _repositoryRoot;
        public FileSystemTestAssembliesRepository(string repositoryRoot){
            _repositoryRoot = repositoryRoot;
        }

        public void MoveToTestSuite(TestSuite suite, string currentLocation)
        {
    	    var dirInfo = new DirectoryInfo(currentLocation);
            string suiteLocation = GetSuiteLocation(suite);

            Directory.CreateDirectory(suiteLocation);

            foreach(FileInfo file in dirInfo.GetFiles())
            {
                file.MoveTo(suiteLocation);
            }

            dirInfo.Delete(true);
        }

        private string GetSuiteLocation(TestSuite suite){
            string safeName = SafeFileName(suite.Name);
            string fullPath = Path.Combine(_repositoryRoot, suite.Id, safeName);
            return fullPath;
        }

        private static string SafeFileName(string name)
        {
            var illegalChars = Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars());
            foreach(var illegalChar in illegalChars)
            {
                name = name.Replace(illegalChar.ToString(), "");
            }
            return name;
        }
    }

    public class CreateProjectValidator : AbstractValidator<Project>
    {
        public CreateProjectValidator()
        {
            RuleFor(p => p.Name).NotEmpty().Length(0, 100);
            RuleFor(p => p.Description).Length(0, 500);
        }
    }

    public class CreateTestSuiteValidator : AbstractValidator<TestSuite>
    {
        public CreateTestSuiteValidator()
        {
            RuleFor(p => p.Name).NotEmpty().Length(0, 100);
            RuleFor(p => p.ProjectId).NotEmpty();
            RuleFor(p => p.Configuration).NotNull();

            RuleFor(p => p.Configuration.Browsers).NotEmpty();
            RuleFor(p => p.Configuration.TestCategoriesQuery).Length(0, 500);

            RuleFor(p => p.Schedule.Time).InclusiveBetween(TimeSpan.Zero, new TimeSpan(23, 59, 59));
            RuleFor(p => p.Schedule.Interval).GreaterThanOrEqualTo(TimeSpan.FromMinutes(5));
        }
    }

    public class MongoDatabaseProvider
    {
        public IMongoDatabase GetDatabase()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase("Autodash");
            
            return db;
        }
    }

}
