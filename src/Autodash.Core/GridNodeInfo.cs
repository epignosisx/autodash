using System.Collections.Generic;
using System.Linq;

namespace Autodash.Core
{
    public class GridNodeInfo
    {
        public List<GridNodeBrowserInfo> Browsers { get; set; }
        public int MaxSessions { get; set; }
        public string Id { get; set; }
        public string Os { get; set; }

        public GridNodeInfo()
        {
            Browsers = new List<GridNodeBrowserInfo>();
        }

        public bool Satisfies(Browser browser, string seleniumProtocol)
        {
            return Browsers.Any(n => n.Protocol == seleniumProtocol && n.Satisfies(browser));
        }
    }
}