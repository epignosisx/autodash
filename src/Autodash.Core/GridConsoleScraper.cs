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

        public async Task<List<GridNodeInfo>> GetAvailableNodesInfoAsync(Uri gridConsoleUrl, IWebProxy proxy = null)
        {
            if (gridConsoleUrl == null)
                throw new ArgumentNullException("gridConsoleUrl");

            string html;
            using (var webClient = new WebClient())
            {
                if (proxy != null)
                    webClient.Proxy = proxy;

                html = await webClient.DownloadStringTaskAsync(gridConsoleUrl);
            }

            var matches = ImgRegex.Matches(html);
            var nodes = new List<GridNodeInfo>(matches.Count);

            foreach (Match m in matches)
            {
                if (m.Groups.Count == 2)
                {
                    string value = m.Groups[1].Value;
                }
            }

            return nodes;
        }

    }
}
