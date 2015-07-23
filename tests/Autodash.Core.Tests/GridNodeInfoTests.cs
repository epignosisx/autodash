using System;
using System.Threading.Tasks;
using Xunit;

namespace Autodash.Core.Tests
{
    public class DefaultGridConsoleScraperTests
    {
        [Fact]
        [Trait("GridHubIntegration", "true")]
        public async Task ReturnsNodesWhenHubIsOpenAndHasRegisteredNodes()
        {
            var scraper = new DefaultGridConsoleScraper();
            var browserNodes = await scraper.GetAvailableNodesInfoAsync(new Uri("http://localhost:4444/grid/console"));
            Assert.True(browserNodes.Count > 0);
        }

        [Fact]
        public async Task ThrowsExceptionWhenHubIsNotReachable()
        {
            var scraper = new DefaultGridConsoleScraper();
            await Assert.ThrowsAsync<GridConsoleScraperException>(async () => await scraper.GetAvailableNodesInfoAsync(new Uri("http://localhost:4444/grid/console")));
        }
    }

    public class GridNodeInfoTests
    {
        public class TryParseTests
        {
            [Theory]
            [InlineData("{seleniumProtocol=Selenium, platform=VISTA, browserName=*iexplore, maxInstances=1}")]
            [InlineData("{seleniumProtocol=Selenium, platform=VISTA, browserName=*firefox, maxInstances=5}")]
            [InlineData("{seleniumProtocol=Selenium, platform=VISTA, browserName=*googlechrome, maxInstances=5}")]
            [InlineData("{seleniumProtocol=WebDriver, platform=VISTA, browserName=chrome, maxInstances=5}")]
            [InlineData("{seleniumProtocol=WebDriver, platform=VISTA, browserName=firefox, maxInstances=5}")]
            [InlineData("{seleniumProtocol=WebDriver, platform=VISTA, browserName=internet explorer, maxInstances=1}")]
            public static void ValidNodeStringsAreParsed(string value)
            {
                GridNodeBrowserInfo browserInfo;
                bool parsed = GridNodeBrowserInfo.TryParse(value, out browserInfo);
                Assert.True(parsed);
                Assert.NotNull(browserInfo.BrowserName);
                Assert.NotNull(browserInfo.Platform);
                Assert.NotNull(browserInfo.Protocol);
                Assert.True(browserInfo.MaxInstances > 0);
            }


            [Theory]
            [InlineData("POST - /session/a1f74fdc-3cc0-4587-957a-050cd3c82754/element/0/value executing ...")]
            [InlineData("some invalid string")]
            public static void InvalidNodeStringIsNotParsed(string value)
            {
                GridNodeBrowserInfo browserInfo;
                bool parsed = GridNodeBrowserInfo.TryParse(value, out browserInfo);
                Assert.False(parsed);
            }
        }
    }
}
