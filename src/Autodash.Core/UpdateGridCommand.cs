using System.Threading.Tasks;
using FluentValidation;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Autodash.Core
{
    public class UpdateGridCommand
    {
        private readonly IMongoDatabase _db;

        public UpdateGridCommand(IMongoDatabase db)
        {
            _db = db;
        }

        public Task ExecuteAsync(SeleniumGridConfiguration config)
        {
            var validator = new UpdateGridConfigValidator();
            validator.ValidateAndThrow(config);

            if (!config.HubUrl.EndsWith("/"))
                config.HubUrl += "/";

            var filter = new BsonDocument();//get all
            return _db.GetCollection<SeleniumGridConfiguration>("SeleniumGridConfiguration")
                .ReplaceOneAsync(filter,  config, new UpdateOptions{IsUpsert = true});
        }
    }
}