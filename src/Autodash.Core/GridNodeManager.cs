using System.Collections.Generic;
using System.Linq;

namespace Autodash.Core
{
    public class GridNodeManager
    {
        private readonly GridNodeInfoWrapper[] _nodes;

        public GridNodeManager(IEnumerable<GridNodeInfo> nodes)
        {
            _nodes = (from node in nodes
                select new GridNodeInfoWrapper
                {
                    Node = node,
                    Browsers = node.Browsers.Where(n => n.Protocol == "WebDriver")
                        .ToDictionary(n => n, n => false)
                }).ToArray();
        }

        public void InUse(GridNodeBrowserInfo browserNode)
        {
            foreach (var node in _nodes)
            {
                if (node.Browsers.ContainsKey(browserNode))
                {
                    node.Browsers[browserNode] = true;
                    return;
                }
            }
        }

        public IEnumerable<GridNodeBrowserInfo> GetAvailableBrowserNodes()
        {
            foreach (var node in _nodes)
            {
                var maxSessions = new Dictionary<string, int>();
                foreach (var browser in node.Browsers.OrderByDescending(n => n.Value))
                {
                    if (browser.Value)
                    {
                        //browser in use. Let's increment browser usage
                        if (maxSessions.ContainsKey(browser.Key.BrowserName))
                            maxSessions[browser.Key.BrowserName]++;
                        else
                            maxSessions.Add(browser.Key.BrowserName, 1);
                    }
                    else if(maxSessions.Count < node.Node.MaxSessions)//Node MaxSessions takes priority over browser instances
                    {
                        int maxInstances;
                        if (maxSessions.TryGetValue(browser.Key.BrowserName, out maxInstances))
                        {
                            if (browser.Key.MaxInstances < maxInstances)
                            {
                                yield return browser.Key;
                            }
                        }
                        else
                        {
                            yield return browser.Key;
                        }
                    }
                }
            }
        }

        private struct GridNodeInfoWrapper
        {
            public GridNodeInfo Node;
            public Dictionary<GridNodeBrowserInfo, bool> Browsers;
        }
    }
}