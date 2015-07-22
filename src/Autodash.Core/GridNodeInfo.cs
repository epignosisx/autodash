using System;
using System.Linq;

namespace Autodash.Core
{
    public class GridNodeInfo
    {
        public string BrowserName { get; set; }
        public string Protocol { get; set; }
        public int MaxInstances { get; set; }
        public string Platform { get; set; }

        public static bool TryParse(string value, out GridNodeInfo nodeInfo)
        {
            nodeInfo = null;
            if (!value.StartsWith("{seleniumProtocol"))
                return false;
            
            var tuples = value.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(n => n.Split(new[] {'='}))
                .Select(n => new { Key = n[0].Replace("{", "").Trim(), Value = n[1].Replace("}", "").Trim() });

            nodeInfo = new GridNodeInfo();
            foreach (var tuple in tuples)
            {
                switch (tuple.Key)
                {
                    case "seleniumProtocol":
                        nodeInfo.Protocol = tuple.Value;
                        break;
                    case "platform":
                        nodeInfo.Platform = tuple.Value;
                        break;
                    case "browserName":
                        nodeInfo.BrowserName = tuple.Value;
                        break;
                    case "maxInstances":
                        nodeInfo.MaxInstances = int.Parse(tuple.Value);
                        break;
                }
            }
            return true;
        }
    }
}