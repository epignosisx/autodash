using System.Collections.Generic;

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
    }
}