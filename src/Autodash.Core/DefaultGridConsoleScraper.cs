using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Autodash.Core
{
    public class DefaultGridConsoleScraper : IGridConsoleScraper
    {
        private readonly static Regex ImgRegex = new Regex("<img.*? title='(.*?)'.*?/>", RegexOptions.Compiled);

        public async Task<List<GridNodeBrowserInfo>> GetAvailableNodesInfoAsync(Uri gridConsoleUrl, IWebProxy proxy = null)
        {
            if (gridConsoleUrl == null)
                throw new ArgumentNullException("gridConsoleUrl");

            string html;
            try
            {
                using (var webClient = new WebClient())
                {
                    if (proxy != null)
                        webClient.Proxy = proxy;

                    html = await webClient.DownloadStringTaskAsync(gridConsoleUrl);
                }
            }
            catch (Exception ex)
            {
                throw new GridConsoleScraperException(gridConsoleUrl, ex);
            }

            var matches = ImgRegex.Matches(html);
            var nodes = new List<GridNodeBrowserInfo>(matches.Count);

            foreach (Match m in matches)
            {
                if (m.Groups.Count == 2)
                {
                    string value = m.Groups[1].Value;
                    GridNodeBrowserInfo nodeBrowser;
                    if (GridNodeBrowserInfo.TryParse(value, out nodeBrowser))
                    {
                        nodes.Add(nodeBrowser);
                    }
                }
            }

            return nodes;
        }

    }
}
