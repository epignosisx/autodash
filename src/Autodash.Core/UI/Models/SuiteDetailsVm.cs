using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Autodash.Core.UI.Models
{
    public class SuiteDetailsVm
    {
        public TestSuite Suite { get; set; }
        public List<SuiteRun> SuiteRuns { get; set; }

        public bool IsBrowserSelected(string browser)
        {
            return Suite.Configuration.Browsers.Contains(browser);
        }

        public string ScheduleTime
        {
            get
            {
                if (Suite.Schedule == null)
                    return "";
                return Suite.Schedule.Time.Hours + ":" + Suite.Schedule.Time.Minutes;
            }
        }

        public string ScheduleInterval
        {
            get
            {
                if (Suite.Schedule == null)
                    return "";
                return Suite.Schedule.Interval.TotalHours.ToString(CultureInfo.InvariantCulture);
            }
        }
    }

    public static class SuiteRunExtensions
    {
        public static string DurationFriendly(this SuiteRun run)
        {
            var duration = run.Duration;
            return duration == TimeSpan.MaxValue ? "" : duration.TotalMinutes.ToString(CultureInfo.InvariantCulture);
        }

        public static string PassedFailed(this SuiteRun run)
        {
            if (run.Result == null)
                return "";
            return string.Format("{0} / {1}", run.Result.PassedTotal, run.Result.FailedTotal);
        }
    }
}
