using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Autodash.Core.Tests
{
    public class CreateSuiteCommandTests
    {
        [Fact]
        public async Task ValidSuiteIsCreated()
        {
            //arrange
            var repo = Substitute.For<ITestAssembliesRepository>();
            var db = await MongoTestDbProvider.GetDatabase();
            var cmd = new CreateSuiteCommand(db, repo, new FakeLoggerProvider());
            var tempFolder = "a temp folder";
            TestSuite suite = new TestSuite();
            suite.Name = "Test Suite";
            suite.ProjectId = "TheProjectId";
            suite.Schedule = new TestSuiteSchedule
            {
                Interval = TimeSpan.FromHours(24),
                Time = new TimeSpan(22, 0, 0)
            };
            suite.Configuration = new TestSuiteConfiguration
            {
                Browsers = new[] { new Browser(BrowserNames.SeleniumIe), new Browser(BrowserNames.SeleniumFirefox) },
                TestAssembliesPath = "Foo\\Bar",
                EnvironmentUrl = "http://localhost:8090"
            };

            //act
            await cmd.ExecuteAsync(suite, tempFolder);

            //assert
            Assert.NotNull(suite.Id);
            Assert.NotEqual(suite.Id.Length, 0);
            repo.Received().MoveToTestSuite(suite, tempFolder);
        }

        [Fact]
        public async Task ValidTestSuiteWithoutConfigurationIsCreated()
        {
            //arrange
            var repo = Substitute.For<ITestAssembliesRepository>();
            var db = await MongoTestDbProvider.GetDatabase();
            var cmd = new CreateSuiteCommand(db, repo, new FakeLoggerProvider());
            var tempFolder = "a temp folder";
            TestSuite suite = new TestSuite();
            suite.Name = "Test Suite";
            suite.ProjectId = "TheProjectId";
            suite.Schedule = null;
            suite.Configuration = new TestSuiteConfiguration
            {
                Browsers = new[] { new Browser(BrowserNames.SeleniumIe), new Browser(BrowserNames.SeleniumFirefox) },
                TestAssembliesPath = "Foo\\Bar",
                EnvironmentUrl = "http://localhost:8090"
            };

            //act
            await cmd.ExecuteAsync(suite, tempFolder);

            //assert
            Assert.NotNull(suite.Id);
            Assert.NotEqual(suite.Id.Length, 0);
            repo.Received().MoveToTestSuite(suite, tempFolder);
        }
    }
}
