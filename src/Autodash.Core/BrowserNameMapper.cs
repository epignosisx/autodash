using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodash.Core
{
    public static class BrowserNames
    {
        public const string UiChrome = "Chrome";
        public const string UiFirefox = "Firefox";
        public const string UiIe = "IE";

        public const string SeleniumChrome = "chrome";
        public const string SeleniumFirefox = "firefox";
        public const string SeleniumIe = "internet explorer";

        public static readonly IReadOnlyDictionary<string, string> UiToSelenium = new Dictionary<string, string>
        {
            {UiChrome, SeleniumChrome},
            {UiIe, SeleniumIe},
            {UiFirefox, SeleniumFirefox},
        };

        public static readonly IReadOnlyDictionary<string, string> SeleniumToUi = new Dictionary<string, string>
        {
            {SeleniumChrome, UiChrome},
            {SeleniumIe, UiIe},
            {SeleniumFirefox, UiFirefox},
        };
    }
}
