using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Autodash.Core
{
    //public class ParallelSuiteRunContext
    //{
    //    public SuiteRun SuiteRun { get; private set; }
    //    public CancellationToken CancellationToken { get; private set; }
    //    public UnitTestCollection[] UnitTestCollections { get; private set; }
    //    public TaskCompletionSource<SuiteRun> TaskCompletionSource { get; private set; }

    //    public ParallelSuiteRunContext(SuiteRun suiteRun,
    //        CancellationToken cancellationToken,
    //        UnitTestCollection[] unitTestCollections,
    //        TaskCompletionSource<SuiteRun> taskCompletionSource)
    //    {
    //        SuiteRun = suiteRun;
    //        CancellationToken = cancellationToken;
    //        UnitTestCollections = unitTestCollections;
    //        TaskCompletionSource = taskCompletionSource;
    //    }

    //    public Tuple<UnitTestCollection, UnitTestInfo, UnitTestResult, GridNodeBrowserInfo> FindNextTestToRun(GridNodeManager gridNodeManager)
    //    {
    //        var suiteRun = SuiteRun;
    //        var config = suiteRun.TestSuiteSnapshot.Configuration;

    //        var browserNodes = gridNodeManager.GetAvailableBrowserNodes().ToList();

    //        if (config.Browsers.All(b => browserNodes.All(bn => bn.BrowserName != b)))
    //            return null;

    //        for (int i = 0; i < suiteRun.Result.CollectionResults.Count; i++)
    //        {
    //            UnitTestCollectionResult collResult = suiteRun.Result.CollectionResults[i];
    //            var tests = collResult.GetPendingTestsToRun(config.Browsers, config.RetryAttempts);
    //            foreach (var test in tests)
    //            {
    //                for (int j = 0; j < browserNodes.Count; j++)
    //                {
    //                    var browserNode = browserNodes[j];
    //                    if (test.Browsers.Contains(browserNode.BrowserName))
    //                    {
    //                        return FindUnitTestCollection(test.UnitTestResult, browserNode);
    //                    }
    //                }
    //            }
    //        }

    //        return null;
    //    }

    //    private Tuple<UnitTestCollection, UnitTestInfo, UnitTestResult, GridNodeBrowserInfo> FindUnitTestCollection(UnitTestResult unitTestResult, GridNodeBrowserInfo browserNode)
    //    {
    //        var q = from testColl in UnitTestCollections
    //                from test in testColl.Tests
    //                where test.TestName == unitTestResult.TestName
    //                select Tuple.Create(testColl, test, unitTestResult, browserNode);

    //        return q.First();
    //    }
    //}
}
