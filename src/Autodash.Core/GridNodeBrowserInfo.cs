using System;
using System.Linq;

namespace Autodash.Core
{
    public class GridNodeBrowserInfo
    {
        public string BrowserName { get; set; }
        public string Protocol { get; set; }
        public int MaxInstances { get; set; }
        public string Platform { get; set; }

        public static bool TryParse(string value, out GridNodeBrowserInfo nodeBrowserInfo)
        {
            nodeBrowserInfo = null;
            if (!value.StartsWith("{seleniumProtocol"))
                return false;
            
            var tuples = value.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(n => n.Split(new[] {'='}))
                .Select(n => new { Key = n[0].Replace("{", "").Trim(), Value = n[1].Replace("}", "").Trim() });

            nodeBrowserInfo = new GridNodeBrowserInfo();
            foreach (var tuple in tuples)
            {
                switch (tuple.Key)
                {
                    case "seleniumProtocol":
                        nodeBrowserInfo.Protocol = tuple.Value;
                        break;
                    case "platform":
                        nodeBrowserInfo.Platform = tuple.Value;
                        break;
                    case "browserName":
                        nodeBrowserInfo.BrowserName = tuple.Value;
                        break;
                    case "maxInstances":
                        nodeBrowserInfo.MaxInstances = int.Parse(tuple.Value);
                        break;
                }
            }
            return true;
        }
    }
}