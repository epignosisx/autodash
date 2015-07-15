using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
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

        [Fact]
        public void NotQueryIsValid()
        {
            bool result = UnitTestTagSelector.Evaluate("NOT Foo", new[] { "Bar" });
            Assert.True(result);
        }

        [Fact]
        public void NotWithBangCharQueryIsValid()
        {
            bool result = UnitTestTagSelector.Evaluate("!Foo", new[] { "Bar" });
            Assert.True(result);
        }

        [Fact]
        public void AndOperatorAndNotOperatorQueryIsValid()
        {
            bool result = UnitTestTagSelector.Evaluate("Bar AND NOT Foo", new[] { "Bar" });
            Assert.True(result);
        }

        [Theory]
        [InlineData("(Bar && Zar) || !Zar", new[] { "Bar", "Zar" })]
        [InlineData("(Bar AND Zar) OR NOT Zar", new[] { "Bar", "Zar" })]
        [InlineData("(Bar && Zar) || !Zar", new[] { "Foo" })]
        [InlineData("(Bar AND Zar) OR NOT Zar", new[] { "Foo" })]
        [InlineData("!Bar && !Zar", new[] { "Foo" })]
        [InlineData("NOT Bar AND NOT Zar", new[] { "Foo" })]
        public void ComplexQueryShouldBeValid(string query, string[] tags)
        {
            bool result = UnitTestTagSelector.Evaluate(query, tags);
            Assert.True(result);
        }
    }
}
