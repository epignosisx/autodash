using System.Threading.Tasks;
using FluentValidation;
using MongoDB.Driver;

namespace Autodash.Core
{
    public class CreateProjectCommand
    {
        private readonly IMongoDatabase _db;
        
        public CreateProjectCommand(IMongoDatabase db)
        {
            _db = db;
        }

        public Task ExecuteAsync(Project project)
        {
            var validator = new CreateProjectValidator();
            validator.ValidateAndThrow(project);

            var coll = _db.GetCollection<Project>("Project");
            return coll.InsertOneAsync(project);
        }
    }
}