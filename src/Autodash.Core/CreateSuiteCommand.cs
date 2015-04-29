using System.Threading.Tasks;
using FluentValidation;
using MongoDB.Driver;

namespace Autodash.Core
{
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
}