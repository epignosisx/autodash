using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Autodash.Core.Tests
{
    public class MsTestRunnerTests
    {
        [Fact]
        public async Task PassingTestExecutesAndPassedResultIsReturned()
        {
            MsTestRunner subject = new MsTestRunner();
            UnitTestInfo unitTest = new UnitTestInfo("Autodash.MsTest.ValidTests.UnitTest1.SuccessTest", null);
            UnitTestCollection coll = new UnitTestCollection("Autodash.MsTest.ValidTests", "Autodash.MsTest.ValidTests.dll", new []{unitTest}, subject);
            TestSuiteConfiguration config = new TestSuiteConfiguration{
                Browsers = new []{"Chrome"},
                TestAssembliesPath = Environment.CurrentDirectory
            };

            UnitTestResult result = await subject.Run(unitTest, coll, config);

            Assert.True(result.Passed);
            Assert.False(File.Exists(Path.Combine(Environment.CurrentDirectory, "Autodash.MsTest.ValidTests.UnitTest1.SuccessTest_Chrome.bat")));
            Assert.False(File.Exists(Path.Combine(Environment.CurrentDirectory, "Autodash.MsTest.ValidTests.UnitTest1.SuccessTest_Chrome.trx")));
        }

        [Fact]
        public async Task FailingTestExecutesAndFailedResultIsReturned()
        {
            MsTestRunner subject = new MsTestRunner();
            UnitTestInfo unitTest = new UnitTestInfo("Autodash.MsTest.ValidTests.UnitTest1.FailTest", null);
            UnitTestCollection coll = new UnitTestCollection("Autodash.MsTest.ValidTests", "Autodash.MsTest.ValidTests.dll", new[] { unitTest }, subject);
            TestSuiteConfiguration config = new TestSuiteConfiguration
            {
                Browsers = new[] { "Chrome" },
                TestAssembliesPath = Environment.CurrentDirectory
            };

            UnitTestResult result = await subject.Run(unitTest, coll, config);

            Assert.False(result.Passed);
            Assert.False(File.Exists(Path.Combine(Environment.CurrentDirectory, "Autodash.MsTest.ValidTests.UnitTest1.SuccessTest_Chrome.bat")));
            Assert.False(File.Exists(Path.Combine(Environment.CurrentDirectory, "Autodash.MsTest.ValidTests.UnitTest1.SuccessTest_Chrome.trx")));
        }

        [Fact]
        public async Task PassingTestExecutesAndPassedResultIsReturnedForThreeBrowsers()
        {
            MsTestRunner subject = new MsTestRunner();
            UnitTestInfo unitTest = new UnitTestInfo("Autodash.MsTest.ValidTests.UnitTest1.SuccessTest", null);
            UnitTestCollection coll = new UnitTestCollection("Autodash.MsTest.ValidTests", "Autodash.MsTest.ValidTests.dll", new[] { unitTest }, subject);
            TestSuiteConfiguration config = new TestSuiteConfiguration
            {
                Browsers = new[] { "Chrome", "Firefox", "Internet Explorer" },
                TestAssembliesPath = Environment.CurrentDirectory
            };

            UnitTestResult result = await subject.Run(unitTest, coll, config);

            Assert.True(result.Passed);
        }
    }
}
