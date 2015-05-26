using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Autodash.Core
{
    public class MsTestDiscoverer : IUnitTestDiscoverer
    {
        private const string TestClassFullName = "Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute";
        private const string TestMethodFullName = "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute";
        private const string TestCategoryFullName = "Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute";
        private const string IgnoreFullName = "Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute";

        private readonly MsTestRunner _runner = new MsTestRunner();

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
                        if(method.IsPublic && HasCustomAttribute(method, TestMethodFullName) && !HasCustomAttribute(method, IgnoreFullName))
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