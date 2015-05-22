using System.Collections.Generic;

namespace Autodash.Core
{
    public interface ITestSuiteUnitTestDiscoverer
    {
        IEnumerable<UnitTestCollection> Discover(string testAssembliesPath);
    }
}