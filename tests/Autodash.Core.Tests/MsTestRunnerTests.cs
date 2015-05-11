using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Autodash.Core.Tests
{
    public class MsTestRunnerTests
    {
        [Fact]
        public void TestAssemblyRunsSuccessfully()
        {
            MsTestRunner subject = new MsTestRunner();
            UnitTestInfo unitTest = new UnitTestInfo("Autodash.MsTest.ValidTests.UnitTest1.SuccessTest", null);
            UnitTestCollection coll = new UnitTestCollection("Autodash.MsTest.ValidTests", "Autodash.MsTest.ValidTests.dll", new []{unitTest}, subject);
            TestSuiteConfiguration config = new TestSuiteConfiguration{
                Browsers = new []{"Chrome"},
                TestAssembliesPath = Environment.CurrentDirectory
            };

            UnitTestResult result = subject.Run(unitTest, coll, config);

            Assert.NotNull(result);
        }
    }
}
