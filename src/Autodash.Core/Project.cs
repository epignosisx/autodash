using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Autodash.Core
{
    public class Project
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string[] MemberEmails { get; set; }
    }
}