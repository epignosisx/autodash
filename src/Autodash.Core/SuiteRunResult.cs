using MongoDB.Bson.Serialization.Attributes;
namespace Autodash.Core
{
    public abstract class SuiteRunResult
    {
        public bool Passed { get; set; }

        public string Status { get; set; }

        public string Details { get; set; }
    }

    [BsonDiscriminator("FailedToStartSuiteRunResult")]
    public class FailedToStartSuiteRunResult : SuiteRunResult
    {
        public FailedToStartSuiteRunResult(string details)
        {
            Passed = false;
            Status = "Failed to Start";
            Details = details;
        }
    }

    public class RanToCompletionSuiteRunResult : SuiteRunResult
    {
        public List<UnitTestCollectionResult> CollectionResults { get; set; }
    }
}