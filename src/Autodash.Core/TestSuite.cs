using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Autodash.Core
{
    public class TestSuite
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public TestSuiteConfiguration Configuration { get; set; }
        public TestSuiteSchedule Schedule { get; set; }
    }
}