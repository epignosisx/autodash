using System.Threading.Tasks;
using FluentValidation;
using MongoDB.Driver;

namespace Autodash.Core
{
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
}