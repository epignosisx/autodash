using System.Threading.Tasks;
using FluentValidation;
using MongoDB.Driver;

namespace Autodash.Core
{
    public class CreateProjectCommand
    {
        private readonly IMongoDatabase _db;
        private readonly ILoggerWrapper _logger;
        
        public CreateProjectCommand(IMongoDatabase db, ILoggerProvider loggerProvider)
        {
            _db = db;
            _logger = loggerProvider.GetLogger(GetType().Name);
        }

        public async Task ExecuteAsync(Project project)
        {
            var validator = new CreateProjectValidator();
            validator.ValidateAndThrow(project);

            var coll = _db.GetCollection<Project>("Project");
            await coll.InsertOneAsync(project);
            _logger.Info("Project created {0} - {1}", project.Id, project.Name);
        }
    }


    public class UpdateProjectCommand
    {
        private readonly IMongoDatabase _db;
        private readonly ILoggerWrapper _logger;

        public UpdateProjectCommand(IMongoDatabase db, ILoggerProvider loggerProvider)
        {
            _db = db;
            _logger = loggerProvider.GetLogger(GetType().Name);
        }

        public async Task ExecuteAsync(Project project)
        {
            var validator = new UpdateProjectValidator();
            validator.ValidateAndThrow(project);

            var coll = _db.GetCollection<Project>("Project");

            var query = Builders<Project>.Filter.Eq(n => n.Id, project.Id);
            await coll.ReplaceOneAsync(query, project);
            _logger.Info("Project updated {0} - {1}", project.Id, project.Name);
        }
    }
}