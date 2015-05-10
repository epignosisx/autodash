using System.Collections.Generic;

namespace Autodash.Core
{
    public class UnitTestDiscovererProvider
    {
        private readonly List<IUnitTestDiscoverer> _discoverers = new List<IUnitTestDiscoverer>();

        public IList<IUnitTestDiscoverer> Discoverers { get { return _discoverers; } }
    }
}