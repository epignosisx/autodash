using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Autodash.Core
{
    public interface ISuiteRunner
    {
        Task<SuiteRun> Run(SuiteRun run);
    }

    public class UnitTestDiscovererProvider
    {
        private readonly List<IUnitTestDiscoverer> _discoverers = new List<IUnitTestDiscoverer>();

        public IList<IUnitTestDiscoverer> Discoverers { get { return _discoverers; } }
    }

    public static class UnitTestTagSelector
    {
        private static readonly Regex Splitter = new Regex(@"\s+", RegexOptions.Compiled);
        private static readonly string[] BooleanAndExpressions = new[] { "AND", "&&", "&" };
        private static readonly string[] BooleanOrExpressions = new[] { "OR", "||", "|" };

        public static bool Evaluate(string tagFilter, string[] unitTestTags)
        {
            string[] parts = Splitter.Split(tagFilter);
            StringBuilder sb = new StringBuilder();
            foreach (var part in parts)
            {
                string copy = part;
                bool hasLeftParen = copy.StartsWith("(");
                bool hasRightParen = copy.EndsWith(")");

                if (hasLeftParen)
                    copy = copy.Substring(1);

                if (hasRightParen)
                    copy = copy.Substring(0, copy.Length - 1);

                string value = "false";
                if(BooleanAndExpressions.Any(n => string.Equals(n, copy, StringComparison.OrdinalIgnoreCase)))
                {
                    value = "&&";
                }
                else if (BooleanOrExpressions.Any(n => string.Equals(n, copy, StringComparison.OrdinalIgnoreCase)))
                {
                    value = "||";
                }
                else if (unitTestTags.Any(n => string.Equals(n, copy, StringComparison.OrdinalIgnoreCase)))
                {
                    value = "true";
                }

                if(hasLeftParen)
                {
                    sb.Append("(");
                }

                sb.Append(value);
 
                if(hasRightParen)
                {
                    sb.Append(")");
                }

                sb.Append(" ");
            }
        }
    }

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

            string testTagsQuery = run.TestSuiteSnapshot.Configuration.TestTagsQuery;
            string[] browsers = run.TestSuiteSnapshot.Configuration.Browsers;
            foreach (var testColl in testColls)
            {
                foreach (var test in testColl.Tests)
                {
                    bool shouldRun = UnitTestTagSelector.Evaluate(testTagsQuery, test.TestTags);
                    if(shouldRun)
                    {
                        UnitTestResult result = testColl.Runner.Run(test, testColl, browsers);
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
                    UnitTestCollection testColl = discoverer.DiscoverTests(testAssembly);
                    yield return testColl;
                }
            }
        }
    }

    public interface IUnitTestDiscoverer
    {
        UnitTestCollection DiscoverTests(string assemblyPath);
    }

    public interface IUnitTestRunner
    {
        UnitTestResult Run(UnitTestInfo unitTest, UnitTestCollection testCollection, string[] browsers);
    }

    public class UnitTestResult
    {
    }

    public class UnitTestInfo
    {
        public string TestName { get; private set; }
        public string[] TestTags { get; private set; }

        public UnitTestInfo(string testName, string[] testTags)
        {
            TestName = testName;
            TestTags = testTags;
        }
    }

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

    public class MSTestDiscoverer : IUnitTestDiscoverer
    {
        private const string TestClassFullName = "Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute";
        private const string TestMethodFullName = "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute";
        private const string TestCategoryFullName = "Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute";

        private readonly MSTestRunner _runner = new MSTestRunner();

        public UnitTestCollection DiscoverTests(string assemblyPath)
        {
            ModuleDefinition module = ModuleDefinition.ReadModule(assemblyPath);
            
            var tests = new List<UnitTestInfo>();
            
            foreach (var type in module.Types)
            {
                if (type.IsPublic && HasCustomAttribute(type, TestClassFullName))
                { 
                    foreach(var method in type.Methods)
                    {
                        if(method.IsPublic && HasCustomAttribute(method, TestMethodFullName))
                        {
                            string methodName = method.DeclaringType.FullName + "." + method.Name;
                            string[] testCats = GetTestCategories(method).ToArray();
                            tests.Add(new UnitTestInfo(methodName, testCats));
                        }
                    }
                }
            }

            var testColl = new UnitTestCollection(module.Assembly.FullName, assemblyPath, tests, _runner);
            return testColl;
        }

        private static bool HasCustomAttribute(ICustomAttributeProvider type, string attributeName)
        {
            return type.CustomAttributes.Any(n => n.AttributeType.FullName == attributeName);
        }

        private static IEnumerable<string> GetTestCategories(ICustomAttributeProvider type)
        {
            foreach (var attr in type.CustomAttributes)
            {
                if (attr.AttributeType.FullName == TestCategoryFullName)
                {
                    foreach(var arg in attr.ConstructorArguments)
                    {
                        yield return (string)arg.Value;
                    }
                }
            }
        }
    }
}