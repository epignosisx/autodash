using System.Collections.Generic;
using System.Linq;

namespace Autodash.Core
{
    public class UnitTestCollection
    {
        public string AssemblyName { get; private set; }
        public string AssemblyPath { get; private set; }
        public UnitTestInfo[] Tests { get; private set; }
        public IUnitTestRunner Runner { get; private set; }

        public UnitTestCollection()
        {
            Tests = new UnitTestInfo[0];
        }

        public UnitTestCollection(string assemblyName, string assemblyPath, IEnumerable<UnitTestInfo> tests, IUnitTestRunner runner)
        {
            AssemblyName = assemblyName;
            AssemblyPath = assemblyPath;
            Tests = tests.ToArray();
            Runner = runner;
        }
    }
}