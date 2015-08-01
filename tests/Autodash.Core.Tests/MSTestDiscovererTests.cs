using Xunit;

namespace Autodash.Core.Tests
{
    public class MSTestDiscovererTests
    {
        [Fact]
        public void UnitTestsAreFound()
        {
            var subject = new MsTestDiscoverer();
            UnitTestCollection result = subject.DiscoverTests("Autodash.MsTest.ValidTests.dll");
            Assert.Equal("Autodash.MsTest.ValidTests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", result.AssemblyName);
            Assert.Equal("Autodash.MsTest.ValidTests.dll", result.AssemblyPath);
            Assert.Equal(4, result.Tests.Length);
        }
    }
}
