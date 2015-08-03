using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Autodash.Core
{
    public class SeleniumGridConfiguration
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string HubUrl { get; set; }

        [BsonIgnore]
        public string RemoteWebDriverUrl {
            get { return HubUrl + "wd/hub"; }
        }
        public int MaxParallelTestSuitesRunning { get; set; }
    }
}