using System.Collections.Generic;

namespace Autodash.Core
{
    public class UnitTestDiscovererProvider
    {
        private static readonly List<IUnitTestDiscoverer> _discoverers = new List<IUnitTestDiscoverer>
        {
            new MsTestDiscoverer()
        };

        public static IList<IUnitTestDiscoverer> Discoverers { get { return _discoverers; } }
    }
}