using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Autodash.Core
{
    public class DefaultSuiteRunner : ISuiteRunner
    {
        private readonly UnitTestDiscovererProvider _testDicovererProvider;

        public DefaultSuiteRunner(UnitTestDiscovererProvider testDicovererProvider)
        {
            _testDicovererProvider = testDicovererProvider;
        }

        public async Task<SuiteRun> Run(SuiteRun run)
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
                UnitTestCollectionResult collResult = new UnitTestCollectionResult();
                collResult.AssemblyName = testColl.AssemblyName;
                collResult.UnitTestResults = new List<UnitTestResult>();
                foreach (UnitTestInfo test in testColl.Tests)
                {
                    bool shouldRun = false;
                    try
                    {
                        shouldRun = UnitTestTagSelector.Evaluate(testTagsQuery, test.TestTags);
                    }
                    catch (Exception ex)
                    {
                        run.Result.Status = "Failed to evaluate test tag query";
                        run.Result.Details = "Review test tag query for invalid query." + Environment.NewLine + ex.ToString();
                        return run;
                    }
                    
                    if(shouldRun)
                    {
                        UnitTestResult result = await testColl.Runner.Run(test, testColl, config);
                        collResult.UnitTestResults.Add(result);
                    }
                }

                results[index++] = collResult;
            }
            
            run.Result.Status = "Ran to Completion";
            run.Result.Details = string.Format("Passed: {0}. Failed: {1}", run.Result.PassedTotal, run.Result.FailedTotal);
            return run;
        }

        private void SafeOp(Action op, string status, string details)
        {
            try
            {
                op();
            }
            catch (Exception ex)
            {

                throw;
            }
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
                yield break;
            }

            var testAssemblies = Directory.GetFiles(testAssembliesPath)
                .Where(n => string.Equals(Path.GetExtension(n), ".dll", StringComparison.OrdinalIgnoreCase));

            foreach (var testAssembly in testAssemblies)
            {
                foreach (var discoverer in _testDicovererProvider.Discoverers)
                {
                    string fullpath = Path.Combine(testAssembliesPath, testAssembly);
                    UnitTestCollection testColl = discoverer.DiscoverTests(fullpath);
                    yield return testColl;
                }
            }
        }
    }
}