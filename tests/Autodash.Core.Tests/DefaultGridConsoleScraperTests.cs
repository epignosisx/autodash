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
}