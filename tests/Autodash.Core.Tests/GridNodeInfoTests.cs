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
                GridNodeInfo info;
                bool parsed = GridNodeInfo.TryParse(value, out info);
                Assert.True(parsed);
                Assert.NotNull(info.BrowserName);
                Assert.NotNull(info.Platform);
                Assert.NotNull(info.Protocol);
                Assert.True(info.MaxInstances > 0);
            }
        }
    }
}
