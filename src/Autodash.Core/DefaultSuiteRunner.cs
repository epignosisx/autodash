using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Autodash.Core
{
    //public class DefaultSuiteRunner : ISuiteRunner
    //{
    //    private readonly ITestSuiteUnitTestDiscoverer _unitTestDiscoverer;

    //    public DefaultSuiteRunner(ITestSuiteUnitTestDiscoverer unitTestDiscoverer)
    //    {
    //        _unitTestDiscoverer = unitTestDiscoverer;
    //    }

    //    public async Task<SuiteRun> Run(SuiteRun run, CancellationToken cancellationToken)
    //    {
    //        if (run == null)
    //            throw new ArgumentNullException("run");

    //        UnitTestCollection[] testColls = ValidateRun(run).ToArray();
    //        if (run.Result != null)
    //            return await Task.FromResult(run);

    //        run.Result = new SuiteRunResult();

    //        for (int i = 0; i < testColls.Length;i++)
    //        {
    //            run.Result.CollectionResults.Add(new UnitTestCollectionResult
    //            {
    //                AssemblyName = testColls[i].AssemblyName,
    //                UnitTestResults = new List<UnitTestResult>()
    //            });
    //        }

    //        TestSuiteConfiguration config = run.TestSuiteSnapshot.Configuration;
            
    //        int index = 0;
    //        foreach (UnitTestCollection testColl in testColls)
    //        {
    //            var collResult = run.Result.CollectionResults[index++];
    //            foreach (UnitTestInfo test in testColl.Tests)
    //            {
    //                if (cancellationToken.IsCancellationRequested)
    //                {
    //                    run.Result.Details = "Suite Run was cancelled during execution.";
    //                    return run;
    //                }

    //                bool shouldRun = config.SelectedTests == null || config.ContainsTest(test.TestName);
    //                if(shouldRun)
    //                {
    //                    UnitTestResult result = await testColl.Runner.Run(test, testColl, config, cancellationToken);
    //                    collResult.UnitTestResults.Add(result);
    //                }
    //            }
    //        }

    //        run.Result.Details = "Ran to Completion";
    //        return run;
    //    }

    //    private IEnumerable<UnitTestCollection> ValidateRun(SuiteRun run)
    //    {
    //        var config = run.TestSuiteSnapshot.Configuration;
    //        string testAssembliesPath = config.TestAssembliesPath;
    //        if(!Directory.Exists(testAssembliesPath))
    //        {
    //            run.Result = new SuiteRunResult
    //            {
    //                Details = "Failed to Start. Test assemblies not found at: " + testAssembliesPath
    //            };
    //            return Enumerable.Empty<UnitTestCollection>();
    //        }

    //        return _unitTestDiscoverer.Discover(testAssembliesPath);
    //    }
    //}
}