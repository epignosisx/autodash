using System;

namespace Autodash.Core
{
    public class TestSuiteSchedule
    {
        public TimeSpan Time { get; set; }
        public TimeSpan Interval { get; set; }

        public DateTime GetNextRunDate(DateTime lastRunDate)
        {
            var time = Time;
            var interval = Interval;
            var runDt = new DateTime(lastRunDate.Year, lastRunDate.Month, lastRunDate.Day, time.Hours, time.Minutes, 0);
            while (runDt < lastRunDate)
            {
                runDt = runDt.Add(interval);
            }
            return runDt;
        }

        public override string ToString()
        {
            return string.Format("Runs at {0} every {1}", Time, Interval);
        }
    }
}