using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nancy.Helpers;

namespace Autodash.Core.UI.Models
{
    public class SuiteDetailsVm
    {
        public TestSuite Suite { get; set; }
        public List<SuiteRun> SuiteRuns { get; set; }
        public FileExplorerVm FileExplorer { get; set; }

        public List<KeyValuePair<string, string>> AvailableBrowsers { get; set; }

        public bool IsBrowserSelected(string browserVersion)
        {
            string[] parts = browserVersion.Split('|');
            return Suite.Configuration.Browsers.Any(b => b.Name == parts[0] && (b.Version == null || b.Version == parts[1]));
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

    public class FileExplorerVm
    {
        public List<FileExplorerItem> Files { get; set; }
        public string TestSuiteId { get; set; }
    }

    public class FileExplorerItem
    {
        public string Filename { get; set; }
        public string FileContent { get; set; }
        public string TestSuiteId { get; set; }
        public string EditLink { get; set; }
    }

    public static class SuiteRunExtensions
    {
        public static string DurationFriendly(this SuiteRun run)
        {
            var duration = run.Duration;
            return duration == TimeSpan.MaxValue ? "" : duration.TotalMinutes.ToString("0.00", CultureInfo.InvariantCulture);
        }

        public static string PassedFailedInconclusive(this SuiteRun run)
        {
            if (run.Result == null)
                return "";
            return string.Format("{0}/{1}/{2}", run.Result.PassedTotal, run.Result.FailedTotal, run.Result.InconclusiveTotal);
        }

        public static string StatusColored(this SuiteRun run)
        {
            string title = null;
            if (run.Status == SuiteRunStatus.Complete)
            {
                if(run.Result.Outcome == TestOutcome.Passed)
                    title = "<span class='label label-success' title='{0}'>Complete</span>";
                else if (run.Result.Outcome == TestOutcome.Inconclusive)
                    title = "<span class='label label-warning' title='{0}'>Complete</span>";
                else if (run.Result.Outcome == TestOutcome.Failed)
                    title = "<span class='label label-danger' title='{0}'>Complete</span>";
            }
            else if (run.Status == SuiteRunStatus.Scheduled)
            {
                title = "<span class='label label-info' title='{0}'>Scheduled</span>";
            }
            else if (run.Status == SuiteRunStatus.Running)
            {
                title = "<span class='label label-primary' title='{0}'>Running</span>";
            }

            if(!string.IsNullOrEmpty(title))
                return string.Format(title, HttpUtility.HtmlAttributeEncode(run.Result == null ? "" : run.Result.Details));
            return "";
        }
    }
}
