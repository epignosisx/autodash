using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Autodash.Core
{
    public class TestRunContext
    {
        public UnitTestInfo UnitTestInfo { get; private set; }
        public UnitTestCollection UnitTestCollection { get; private set; }
        public TestSuiteConfiguration TestSuiteConfiguration { get; private set; }
        public GridNodeBrowserInfo GridNodeBrowserInfo { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public TestRunContext(
            UnitTestInfo unitTestInfo,
            UnitTestCollection unitTestCollection,
            TestSuiteConfiguration testSuiteConfiguration,
            GridNodeBrowserInfo gridNodeBrowserInfo,
            CancellationToken cancellationToken)
        {
            UnitTestInfo = unitTestInfo;
            UnitTestCollection = unitTestCollection;
            TestSuiteConfiguration = testSuiteConfiguration;
            GridNodeBrowserInfo = gridNodeBrowserInfo;
            CancellationToken = cancellationToken;
        }
    }
}
