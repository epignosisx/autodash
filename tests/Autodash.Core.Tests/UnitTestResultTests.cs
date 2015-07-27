using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Autodash.Core.Tests
{
    public class UnitTestResultTests
    {
        private readonly string[] Browsers = new string[] { 
            "Firefox", "Chrome"
        };
        [Fact]
        public void NoResultsReturnsAllBrowsers()
        {
            var subject = new UnitTestResult();
            var result = subject.GetPendingBrowserResults(Browsers, 3).ToArray();
            Assert.Equal(result.Length, Browsers.Length);
            Assert.Equal(result[0], Browsers[0]);
            Assert.Equal(result[1], Browsers[1]);
        }

        [Fact]
        public void FailedBrowserTestIsReturnedWithNotExecutedBrowsers()
        {
            var subject = new UnitTestResult();
            subject.BrowserResults.Add(new UnitTestBrowserResult { 
                Attempt = 1,
                Browser = Browsers[0],
                Passed = false
            });
            var result = subject.GetPendingBrowserResults(Browsers, 3).ToArray();
            Assert.Equal(result.Length, Browsers.Length);
            Assert.Equal(result[0], Browsers[0]);
            Assert.Equal(result[1], Browsers[1]);
        }

        [Fact]
        public void FailedBrowserTestPastRetriesIsNotReturned()
        {
            var subject = new UnitTestResult();
            subject.BrowserResults.Add(new UnitTestBrowserResult
            {
                Attempt = 3,
                Browser = Browsers[0],
                Passed = false
            });
            var result = subject.GetPendingBrowserResults(Browsers, 3).ToArray();
            Assert.Equal(result.Length, 1);
            Assert.Equal(result[0], Browsers[1]);
        }

        [Fact]
        public void PassedBrowserTestIsNotReturned()
        {
            var subject = new UnitTestResult();
            subject.BrowserResults.Add(new UnitTestBrowserResult
            {
                Attempt = 1,
                Browser = Browsers[0],
                Passed = true
            });
            var result = subject.GetPendingBrowserResults(Browsers, 3).ToArray();
            Assert.Equal(result.Length, 1);
            Assert.Equal(result[0], Browsers[1]);
        }

        [Fact]
        public void AllPassedBrowserTestAreNotReturned()
        {
            var subject = new UnitTestResult();
            subject.BrowserResults.Add(new UnitTestBrowserResult
            {
                Attempt = 1,
                Browser = Browsers[0],
                Passed = true
            });
            subject.BrowserResults.Add(new UnitTestBrowserResult
            {
                Attempt = 1,
                Browser = Browsers[1],
                Passed = true
            });
            var result = subject.GetPendingBrowserResults(Browsers, 3).ToArray();
            Assert.Equal(result.Length, 0);
        }
    }
}
