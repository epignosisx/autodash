using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Autodash.Core
{
    public class UnitTestBrowserResult
    {
        public Browser Browser { get; set; }
        public string Stdout { get; set; }
        public string Stderr { get; set; }
        public bool Passed { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        [BsonIgnore]
        public TimeSpan Duration { get { return EndTime - StartTime; } }

        public override string ToString()
        {
            return Browser + " - Passed: " + Passed;
        }
    }
}