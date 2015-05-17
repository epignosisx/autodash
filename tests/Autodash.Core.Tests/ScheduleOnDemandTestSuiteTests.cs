using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Autodash.Core.Tests
{
    public class ScheduleOnDemandTestSuiteTests
    {
        [Fact]
        public void ValidTestSuiteIsAddedToScheduler()
        {
            //arrange
            var scheduler = Substitute.For<ISuiteRunScheduler>();
            var cmd = new ScheduleOnDemandTestSuiteCommand(scheduler);
            var suite = new TestSuite();

            //act
            cmd.Execute(suite);

            //assert
            scheduler.Received().Schedule(suite);
        }

        [Fact]
        public void NullTestSuiteThrowsArgumentNullException()
        {
            //arrange
            var scheduler = Substitute.For<ISuiteRunScheduler>();
            var cmd = new ScheduleOnDemandTestSuiteCommand(scheduler);

            //act & assert
            Assert.Throws<ArgumentNullException>(() => cmd.Execute(null));
        }
    }
}
