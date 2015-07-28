using System.Threading;

namespace Autodash.Core
{
    public class TestRunContext
    {
        public UnitTestInfo UnitTestInfo { get; private set; }
        public UnitTestCollection UnitTestCollection { get; private set; }
        public UnitTestResult UnitTestResult { get; set; }
        public TestSuiteConfiguration TestSuiteConfiguration { get; private set; }
        public GridNodeBrowserInfo GridNodeBrowserInfo { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public TestRunContext(
            UnitTestInfo unitTestInfo,
            UnitTestCollection unitTestCollection,
            UnitTestResult unitTestResult,
            TestSuiteConfiguration testSuiteConfiguration,
            GridNodeBrowserInfo gridNodeBrowserInfo,
            CancellationToken cancellationToken)
        {
            UnitTestInfo = unitTestInfo;
            UnitTestCollection = unitTestCollection;
            UnitTestResult = unitTestResult;
            TestSuiteConfiguration = testSuiteConfiguration;
            GridNodeBrowserInfo = gridNodeBrowserInfo;
            CancellationToken = cancellationToken;
        }
    }
}
