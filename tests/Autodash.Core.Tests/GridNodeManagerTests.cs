using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Autodash.Core.Tests
{
    public class GridNodeManagerTests
    {
        private static IEnumerable<GridNodeInfo> GetNodes()
        {
            return new GridNodeInfo[]
            {
                new GridNodeInfo
                {
                    MaxSessions = 2,
                    Browsers = new List<GridNodeBrowserInfo>
                    {
                        new GridNodeBrowserInfo
                        {
                            BrowserName = "firefox",
                            Protocol = "WebDriver",
                            MaxInstances = 2
                        },
                        new GridNodeBrowserInfo
                        {
                            BrowserName = "internetexplorer",
                            Protocol = "WebDriver",
                            MaxInstances = 1
                        },
                        new GridNodeBrowserInfo
                        {
                            BrowserName = "*iexplorer",
                            Protocol = "RemoteControl",
                            MaxInstances = 1
                        }
                    }
                }
            };
        }
            
        [Fact]
        public void OnlyWebDriverBrowserNodesAreKept()
        {
            //arrange
            IEnumerable<GridNodeInfo> nodes = GetNodes();
            GridNodeManager subject = new GridNodeManager(nodes);

            //act
            GridNodeBrowserInfo[] result = subject.GetAvailableBrowserNodes().ToArray();

            Assert.Equal(result.Length, 2);
            Assert.Null(result.FirstOrDefault(n => n.Protocol != "WebDriver"));
        }

        [Fact]
        public void WhenBrowserHasBeenUsedItDoesAppearInAvailableBrowsers()
        {
            //arrange
            IEnumerable<GridNodeInfo> nodes = GetNodes();
            GridNodeManager subject = new GridNodeManager(nodes);

            //act
            GridNodeBrowserInfo[] result = subject.GetAvailableBrowserNodes().ToArray();
            subject.Book(result[0]);
            result = subject.GetAvailableBrowserNodes().ToArray();

            Assert.Equal(result.Length, 1);
        }
    }
}
