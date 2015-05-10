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

        public Task<SuiteRun> Run(SuiteRun run)
        {
            if (run == null)
                throw new ArgumentNullException("run");

            UnitTestCollection[] testColls = ValidateRun(run).ToArray();
            if (run.Result != null)
                return Task.FromResult(run);

            TestSuiteConfiguration config = run.TestSuiteSnapshot.Configuration;
            string testTagsQuery = config.TestTagsQuery;
            foreach (var testColl in testColls)
            {
                foreach (var test in testColl.Tests)
                {
                    bool shouldRun = UnitTestTagSelector.Evaluate(testTagsQuery, test.TestTags);
                    if(shouldRun)
                    {
                        UnitTestResult result = testColl.Runner.Run(test, testColl, config);
                    }
                }
            }

            return Task.FromResult(run);
        }

        private IEnumerable<UnitTestCollection> ValidateRun(SuiteRun run)
        {
            var config = run.TestSuiteSnapshot.Configuration;
            string testAssembliesPath = config.TestAssembliesPath;
            if(!Directory.Exists(testAssembliesPath))
            {
                run.Result = new FailedToStartSuiteRunResult("Test assemblies not found at: " + testAssembliesPath);
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