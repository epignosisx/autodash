using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Autodash.Core
{
    public class UnitTestBrowserResult
    {
        public Browser Browser { get; set; }
        public string Stdout { get; set; }
        public string Stderr { get; set; }
        public TestOutcome Outcome { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        [BsonIgnore]
        public TimeSpan Duration { get { return EndTime - StartTime; } }

        public override string ToString()
        {
            return Browser + " - Outcome: " + Outcome;
        }
    }

    public enum TestOutcome
    {
        Failed = 0,
        Passed = 1,
        Inconclusive = 2
    }
}