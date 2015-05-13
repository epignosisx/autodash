using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xunit;
using NSubstitute;

namespace Autodash.Core.Tests
{
    public class DefaultSuiteRunSchedulerTests
    {
        [Fact]
        public async Task StartingSchedulerWithEmptyQueueDoesNotThrow()
        {
            //arrange
            var runner = Substitute.For<ISuiteRunner>();
            var repo = Substitute.For<ISuiteRunSchedulerRepository>();
            repo.GetTestSuitesWithScheduleAsync().Returns(Task.FromResult(new List<TestSuite>()));
            repo.GetScheduledSuiteRunsAsync().Returns(Task.FromResult(new List<SuiteRun>()));
            
            var subject = new DefaultSuiteRunScheduler(repo, runner);

            //act
            await subject.Start();

            //assert
            repo.Received().GetTestSuitesWithScheduleAsync().IgnoreAwaitForNSubstituteAssertion();
            repo.Received().FailRunningSuitesAsync().IgnoreAwaitForNSubstituteAssertion();
            repo.Received().GetScheduledSuiteRunsAsync().IgnoreAwaitForNSubstituteAssertion();
        }

        [Fact]
        public async Task StartingSchedulerWithOneSuiteWithScheduleForFutureTimeDoesNotCallRunner()
        {
            //arrange
            var runner = Substitute.For<ISuiteRunner>();
            var repo = Substitute.For<ISuiteRunSchedulerRepository>();
            var testSuite = new TestSuite
            {
                Schedule = new TestSuiteSchedule { 
                    Time = new TimeSpan(DateTime.UtcNow.Hour, DateTime.UtcNow.Minute + 1, 0),
                    Interval = TimeSpan.FromDays(1)
                }
            };
            repo.GetTestSuitesWithScheduleAsync().Returns(Task.FromResult(new List<TestSuite> { testSuite }));
            repo.GetScheduledSuiteRunsAsync().Returns(Task.FromResult(new List<SuiteRun>()));

            var subject = new DefaultSuiteRunScheduler(repo, runner);

            //act
            await subject.Start();

            //assert
            repo.Received().GetTestSuitesWithScheduleAsync().IgnoreAwaitForNSubstituteAssertion();
            repo.Received().FailRunningSuitesAsync().IgnoreAwaitForNSubstituteAssertion();
            repo.Received().GetScheduledSuiteRunsAsync().IgnoreAwaitForNSubstituteAssertion();
            runner.DidNotReceive().Run(Arg.Any<SuiteRun>()).IgnoreAwaitForNSubstituteAssertion(); ;
        }

        [Fact]
        public async Task StartingSchedulerWithOneSuiteWithScheduleForPresentTimeCallsRunner()
        {
            //arrange
            var runner = Substitute.For<ISuiteRunner>();
            var repo = Substitute.For<ISuiteRunSchedulerRepository>();
            var testSuite = new TestSuite
            {
                Schedule = new TestSuiteSchedule
                {
                    Time = new TimeSpan(DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0),
                    Interval = TimeSpan.FromDays(1)
                }
            };
            repo.GetTestSuitesWithScheduleAsync().Returns(Task.FromResult(new List<TestSuite> { testSuite }));
            repo.GetScheduledSuiteRunsAsync().Returns(Task.FromResult(new List<SuiteRun>()));

            var subject = new DefaultSuiteRunScheduler(repo, runner);

            //act
            await subject.Start();

            //assert
            repo.Received().GetTestSuitesWithScheduleAsync().IgnoreAwaitForNSubstituteAssertion();
            repo.Received().FailRunningSuitesAsync().IgnoreAwaitForNSubstituteAssertion();
            repo.Received().GetScheduledSuiteRunsAsync().IgnoreAwaitForNSubstituteAssertion();
            runner.Received().Run(Arg.Any<SuiteRun>()).IgnoreAwaitForNSubstituteAssertion();
        }
        
    }
}
