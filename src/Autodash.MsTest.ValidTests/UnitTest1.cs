using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Autodash.MsTest.ValidTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        [TestCategory("Tag1")]
        [TestCategory("Tag2")]
        public void SuccessTest()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void FailTest()
        {
            Assert.IsTrue(false);
        }
    }
}
