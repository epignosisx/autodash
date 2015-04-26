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
            var cmd = new CreateSuiteCommand();
            var tempFolder = "a temp folder";
            TestSuite suite = new TestSuite();
            suite.Name = "Test Suite";
            suite.ProjectId = "TheProjectId";
            suite.Schedule = new TestSuiteSchedule { 
                Interval = TimeSpan.FromHours(24),
                Time = new TimeSpan(22, 0, 0)
            };
            suite.Configuration = new TestSuiteConfiguration {
                Browsers = new[] { "Internet Explorer", "Firefox" },
                TestAssembliesPath = "Foo\\Bar",
                TestCategoriesQuery = "P1 & R3"
            };

            //act
            await cmd.ExecuteAsync(db, repo, suite, tempFolder);

            //assert
            Assert.NotNull(suite.Id);
            Assert.NotEqual(suite.Id.Length, 0);
            repo.Received().MoveToTestSuite(suite, tempFolder);
        }
    }
}
