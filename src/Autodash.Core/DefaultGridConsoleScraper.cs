using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Autodash.Core
{
    public class DefaultGridConsoleScraper : IGridConsoleScraper
    {
        public async Task<List<GridNodeInfo>> GetAvailableNodesInfoAsync(Uri gridConsoleUrl, IWebProxy proxy = null)
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

            var nodes = ParseHtml(html);

            return nodes;
        }

        public List<GridNodeInfo> GetAvailableNodesInfo(Uri gridConsoleUrl, IWebProxy proxy = null)
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

                    html = webClient.DownloadString(gridConsoleUrl);
                }
            }
            catch (Exception ex)
            {
                throw new GridConsoleScraperException(gridConsoleUrl, ex);
            }

            var nodes = ParseHtml(html);

            return nodes;
        }

        private static List<GridNodeInfo> ParseHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var nodes = new List<GridNodeInfo>();
            foreach (var nodeDiv in doc.DocumentNode.SelectNodes("//div[@class='proxy']"))
            {
                var node = new GridNodeInfo();
                var proxyId = nodeDiv.SelectSingleNode("//p[@class='proxyid']");
                var imgs = nodeDiv.SelectNodes("//div[@type='browsers']//img");
                var settings = nodeDiv.SelectNodes("//div[@type='config']//p").Select(n => n.InnerText);

                string text = proxyId.InnerText; //id : http://10.240.240.74:5555, OS : VISTA
                var parts = text.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                node.Id = parts[0].Replace("id : ", "");
                node.Os = parts[1].Replace(" OS : ", "");
                node.MaxSessions = int.Parse(settings.First(n => n.StartsWith("maxSession:")).Replace("maxSession:", ""));

                foreach (var img in imgs)
                {
                    GridNodeBrowserInfo nodeBrowser;
                    if (GridNodeBrowserInfo.TryParse(img.Attributes["title"].Value, out nodeBrowser))
                    {
                        node.Browsers.Add(nodeBrowser);
                    }
                }

                nodes.Add(node);
            }
            return nodes;
        }
    }
}
