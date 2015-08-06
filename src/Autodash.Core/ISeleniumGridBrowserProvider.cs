using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodash.Core
{
    public interface ISeleniumGridBrowserProvider
    {
        IEnumerable<Browser> GetBrowsers();
    }

    public class StaticSeleniumGridBrowserProvider : ISeleniumGridBrowserProvider
    {
        public Browser[] Browsers = new Browser[]
        {
            new Browser(BrowserNames.SeleniumChrome), 
            new Browser(BrowserNames.SeleniumFirefox), 
            new Browser(BrowserNames.SeleniumIe, "8"), 
            new Browser(BrowserNames.SeleniumIe, "9"), 
            new Browser(BrowserNames.SeleniumIe, "10"), 
            new Browser(BrowserNames.SeleniumIe, "11")
        };

        public IEnumerable<Browser> GetBrowsers()
        {
            return Browsers;
        }
    }
}
