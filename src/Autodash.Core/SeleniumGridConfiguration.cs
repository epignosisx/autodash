using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Autodash.Core
{
    public class SeleniumGridConfiguration
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string HubUrl { get; set; }
        public int MaxParallelTestSuitesRunning { get; set; }
    }
}