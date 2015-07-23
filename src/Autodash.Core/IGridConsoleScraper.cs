using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Autodash.Core
{
    public interface IGridConsoleScraper
    {
        Task<List<GridNodeBrowserInfo>> GetAvailableNodesInfoAsync(Uri gridConsoleUrl, IWebProxy proxy = null);
    }

    public class GridConsoleScraperException : Exception
    {
        public GridConsoleScraperException(Uri uri, Exception ex) : 
            base("Cannot reach Grid Console at: " + uri.ToString(), ex)
        {
        }
    }
}