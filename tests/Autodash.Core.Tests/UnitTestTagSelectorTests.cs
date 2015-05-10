using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Autodash.Core.Tests
{
    public class UnitTestTagSelectorTests
    {
        [Fact]
        public void AndQueryIsValid()
        {
            bool result = UnitTestTagSelector.Evaluate("Foo AND Bar", new[] {"Foo", "Bar"});
            Assert.True(result);
        }

        [Fact]
        public void OrQueryIsValid()
        {
            bool result = UnitTestTagSelector.Evaluate("Foo Or Bar", new[] { "Foo", "Bar" });
            Assert.True(result);
        }

        [Fact]
        public void ParenthesisQueryIsValid()
        {
            bool result = UnitTestTagSelector.Evaluate("(Foo Or Bar) AND Foo", new[] { "Foo", "Bar" });
            Assert.True(result);
        }

        [Fact]
        public void MissingTagReturnsFalse()
        {
            bool result = UnitTestTagSelector.Evaluate("(Foo Or Bar) AND Zar", new[] { "Foo", "Bar" });
            Assert.False(result);
        }
    }
}
