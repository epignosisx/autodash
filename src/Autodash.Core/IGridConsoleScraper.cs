using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Autodash.Core
{
    public interface IGridConsoleScraper
    {
        Task<List<GridNodeInfo>> GetAvailableNodesInfoAsync(Uri gridConsoleUrl, IWebProxy proxy = null);
        List<GridNodeInfo> GetAvailableNodesInfo(Uri gridConsoleUrl, IWebProxy proxy = null);
    }
}