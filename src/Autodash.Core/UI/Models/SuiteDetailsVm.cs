using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Management;

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
                return Suite.Schedule.Time.Hours.ToString("00", CultureInfo.InvariantCulture) + ":" + 
                    Suite.Schedule.Time.Minutes.ToString("00", CultureInfo.InvariantCulture);
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
            return duration == TimeSpan.MaxValue ? "" : duration.TotalMinutes.ToString("0.00", CultureInfo.InvariantCulture);
        }

        public static string PassedFailed(this SuiteRun run)
        {
            if (run.Result == null)
                return "";
            return string.Format("{0} / {1}", run.Result.PassedTotal, run.Result.FailedTotal);
        }

        public static string StatusColored(this SuiteRun run)
        {
            if (run.Status == SuiteRunStatus.Complete)
            {
                if(run.Result.Passed)
                    return "<span class='label label-success'>Complete</span>";
                return "<span class='label label-danger'>Complete</span>";
            }

            if (run.Status == SuiteRunStatus.Scheduled)
            {
                return "<span class='label label-info'>Scheduled</span>";
            }

            if (run.Status == SuiteRunStatus.Running)
            {
                return "<span class='label label-primary'>Running</span>";
            }

            return "";
        }
    }
}
