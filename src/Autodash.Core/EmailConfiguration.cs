using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Autodash.Core
{
    public class EmailConfiguration
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public string FromEmail { get; set; }

        public string SmtpServer { get; set; }
        public int Port { get; set; }

        public EmailConfiguration()
        {
            Port = 25;
            FromEmail = "noreply@autodash.org";
        }
    }
}