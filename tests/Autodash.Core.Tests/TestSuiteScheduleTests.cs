using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Autodash.Core.Tests
{
    public class TestSuiteScheduleTests
    {
        [Fact]
        public void NoPreviousRunPicksFirstTime()
        {
            var now = new DateTime(2015, 4, 29, 0, 0, 0);
            var subject = new TestSuiteSchedule();
            subject.Time = TimeSpan.Parse("04:00:00");
            DateTime result = subject.GetNextRunDate(now, DateTime.MinValue);
            Assert.Equal(result, new DateTime(now.Year, now.Month, now.Day, subject.Time.Hours, subject.Time.Minutes, 0));
        }

        [Fact]
        public void RecurrentRunPicksFirstTime()
        {
            var now = new DateTime(2015, 4, 29, 0, 0, 0);
            var subject = new TestSuiteSchedule();
            subject.Time = TimeSpan.Parse("04:00:00");
            subject.Interval = TimeSpan.FromHours(1);
            DateTime result = subject.GetNextRunDate(now, DateTime.MinValue);
            Assert.Equal(result, new DateTime(now.Year, now.Month, now.Day, subject.Time.Hours, subject.Time.Minutes, 0));
        }

        [Fact]
        public void RecurrentRunPicksSecondTimeAfterApplyingInterval()
        {
            var now = new DateTime(2015, 4, 29, 4, 30, 0);
            var subject = new TestSuiteSchedule();
            subject.Time = TimeSpan.Parse("04:00:00");
            subject.Interval = TimeSpan.FromHours(1);
            DateTime result = subject.GetNextRunDate(now, DateTime.MinValue);
            Assert.Equal(result, new DateTime(now.Year, now.Month, now.Day, subject.Time.Hours + 1, subject.Time.Minutes, 0));
        }

        [Fact]
        public void RecurrentRunPicksSecondTimeAfterApplyingIntervalWithLastRun()
        {
            var now = new DateTime(2015, 4, 29, 4, 0, 0);
            var subject = new TestSuiteSchedule();
            subject.Time = TimeSpan.Parse("04:00:00");
            subject.Interval = TimeSpan.FromHours(1);
            DateTime result = subject.GetNextRunDate(now, now.AddMonths(-1));
            Assert.Equal(result, new DateTime(now.Year, now.Month, now.Day, subject.Time.Hours, subject.Time.Minutes, 0));
        }
    }
}
