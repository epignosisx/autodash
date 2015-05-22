using System.Globalization;
using System.Linq;

namespace Autodash.Core.UI.Models
{
    public class SuiteDetailsVm
    {
        public TestSuite Suite { get; set; }
        public UnitTestCollection[] UnitTestCollections { get; set; }

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
}
