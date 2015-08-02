using System.Threading.Tasks;
using FluentValidation;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Autodash.Core
{
    public class UpdateGridCommand
    {
        private readonly IMongoDatabase _db;
        private readonly ILoggerWrapper _logger;

        public UpdateGridCommand(IMongoDatabase db, ILoggerProvider loggerProvider)
        {
            _db = db;
            _logger = loggerProvider.GetLogger(GetType().Name);
        }

        public async Task ExecuteAsync(SeleniumGridConfiguration config)
        {
            var validator = new UpdateGridConfigValidator();
            validator.ValidateAndThrow(config);

            if (!config.HubUrl.EndsWith("/"))
                config.HubUrl += "/";

            var filter = new BsonDocument();//get all
            await _db.GetCollection<SeleniumGridConfiguration>("SeleniumGridConfiguration")
                .ReplaceOneAsync(filter,  config, new UpdateOptions{IsUpsert = true});

            _logger.Info("Grid configuration updated. Hub Url: {0}. Max Parallel Suites: {1}", config.HubUrl, config.MaxParallelTestSuitesRunning);
        }
    }
}