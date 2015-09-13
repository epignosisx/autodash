using System.Threading.Tasks;
using FluentValidation;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Autodash.Core
{
    public class UpdateEmailCommand
    {
        private readonly IMongoDatabase _db;
        private readonly ILoggerWrapper _logger;

        public UpdateEmailCommand(IMongoDatabase db, ILoggerProvider loggerProvider)
        {
            _db = db;
            _logger = loggerProvider.GetLogger(GetType().Name);
        }

        public async Task ExecuteAsync(EmailConfiguration config)
        {
            var validator = new UpdateEmailConfigValidator();
            validator.ValidateAndThrow(config);

            var filter = new BsonDocument();//get all
            await _db.GetCollection<EmailConfiguration>("EmailConfiguration")
                .ReplaceOneAsync(filter, config, new UpdateOptions { IsUpsert = true });

            _logger.Info("Email configuration updated. SMTP: {0}. Port: {1}. From: {2}", config.SmtpServer, config.Port, config.FromEmail);
        }
    }
}