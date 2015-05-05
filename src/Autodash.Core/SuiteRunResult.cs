using MongoDB.Bson.Serialization.Attributes;
namespace Autodash.Core
{
    public abstract class SuiteRunResult
    {
        public bool Success { get; set; }

        public string Status { get; set; }

        public string Details { get; set; }
    }

    [BsonDiscriminator(typeof(FailedToStartSuiteRunResult).Name)]
    public class FailedToStartSuiteRunResult : SuiteRunResult
    {
        public FailedToStartSuiteRunResult(string details)
        {
            Success = false;
            Status = "Failed to Start";
            Details = details;
        }
    }
}