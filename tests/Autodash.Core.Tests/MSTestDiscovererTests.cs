using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Assert.Equal(result.AssemblyName, "Autodash.MsTest.ValidTests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            Assert.Equal(result.AssemblyPath, "Autodash.MsTest.ValidTests.dll");
            Assert.Equal(result.Tests.Length, 3);
        }
    }
}
