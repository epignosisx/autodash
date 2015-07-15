using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Autodash.Core
{
    public class DefaultSuiteRunner : ISuiteRunner
    {
        private readonly ITestSuiteUnitTestDiscoverer _unitTestDiscoverer;

        public DefaultSuiteRunner(ITestSuiteUnitTestDiscoverer unitTestDiscoverer)
        {
            _unitTestDiscoverer = unitTestDiscoverer;
        }

        public async Task<SuiteRun> Run(SuiteRun run, CancellationToken cancellationToken)
        {
            if (run == null)
                throw new ArgumentNullException("run");

            UnitTestCollection[] testColls = ValidateRun(run).ToArray();
            if (run.Result != null)
                return await Task.FromResult(run);

            TestSuiteConfiguration config = run.TestSuiteSnapshot.Configuration;
            string testTagsQuery = config.TestTagsQuery;
            
            var results = new UnitTestCollectionResult[testColls.Length];

            run.Result = new SuiteRunResult();
            run.Result.CollectionResults = results;

            int index = 0;
            foreach (UnitTestCollection testColl in testColls)
            {
                var collResult = new UnitTestCollectionResult();
                collResult.AssemblyName = testColl.AssemblyName;
                collResult.UnitTestResults = new List<UnitTestResult>();
                foreach (UnitTestInfo test in testColl.Tests)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        run.Result.Status = "Suite Run was cancelled.";
                        run.Result.Details = "Suite Run was cancelled during execution.";
                        return run;
                    }

                    bool shouldRun = false;
                    try
                    {
                        shouldRun = string.IsNullOrEmpty(testTagsQuery) || UnitTestTagSelector.Evaluate(testTagsQuery, test.TestTags);
                    }
                    catch (Exception ex)
                    {
                        run.Result.Status = "Failed to evaluate test tag query";
                        run.Result.Details = "Review test tag query for invalid query." + Environment.NewLine + ex.ToString();
                        return run;
                    }
                    
                    if(shouldRun)
                    {
                        UnitTestResult result = await testColl.Runner.Run(test, testColl, config, cancellationToken);
                        collResult.UnitTestResults.Add(result);
                    }
                }

                results[index++] = collResult;
            }
            
            run.Result.Status = "Ran to Completion";
            run.Result.Details = string.Format("Passed: {0}. Failed: {1}", run.Result.PassedTotal, run.Result.FailedTotal);
            return run;
        }

        private IEnumerable<UnitTestCollection> ValidateRun(SuiteRun run)
        {
            var config = run.TestSuiteSnapshot.Configuration;
            string testAssembliesPath = config.TestAssembliesPath;
            if(!Directory.Exists(testAssembliesPath))
            {
                run.Result = new SuiteRunResult();
                run.Result.Status = "Failed to Start";
                run.Result.Details = "Test assemblies not found at: " + testAssembliesPath;
                return Enumerable.Empty<UnitTestCollection>();
            }

            return _unitTestDiscoverer.Discover(testAssembliesPath);
        }
    }
}