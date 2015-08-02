using Xunit;

namespace Autodash.Core.Tests
{
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
