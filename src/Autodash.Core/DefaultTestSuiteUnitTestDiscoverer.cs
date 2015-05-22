using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Autodash.Core
{
    public class DefaultTestSuiteUnitTestDiscoverer : ITestSuiteUnitTestDiscoverer
    {
        public IEnumerable<UnitTestCollection> Discover(string testAssembliesPath)
        {
            var testAssemblies = Directory.GetFiles(testAssembliesPath)
                .Where(n => string.Equals(Path.GetExtension(n), ".dll", StringComparison.OrdinalIgnoreCase));

            var discoverers = UnitTestDiscovererProvider.Discoverers.ToArray();
            foreach (var testAssembly in testAssemblies)
            {
                foreach (var discoverer in discoverers)
                {
                    string fullpath = Path.Combine(testAssembliesPath, testAssembly);
                    UnitTestCollection testColl = discoverer.DiscoverTests(fullpath);
                    yield return testColl;
                }
            }
        }
    }
}