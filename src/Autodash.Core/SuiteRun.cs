using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Globalization;

namespace Autodash.Core
{
    public class SuiteRun
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public DateTime ScheduledFor { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime CompletedOn { get; set; }
        public SuiteRunStatus Status { get; set; }
        public string TestSuiteId { get; set; }
        public SuiteRunResult Result { get; set; }
        public TestSuite TestSuiteSnapshot { get; set; }

        public TimeSpan Duration 
        { 
            get 
            { 
                return CompletedOn == DateTime.MinValue ? TimeSpan.MaxValue : (CompletedOn - StartedOn); 
            }
        }

        public static SuiteRun CreateSuiteRun(TestSuite suite, DateTime scheduledOn)
        {
            var run = new SuiteRun
            {
                ScheduledFor = scheduledOn,
                Status = SuiteRunStatus.Running,
                TestSuiteId = suite.Id,
                TestSuiteSnapshot = suite
            };
            return run;
        }
    }
}