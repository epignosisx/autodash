using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodash.MsTest.ValidTests
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void SuccessTest()
        {
            Console.WriteLine("Running UnitTest2.SuccessTest");
            Assert.IsTrue(true);
        }

        [TestMethod, Ignore]
        public void IgnoredTest()
        {
            Assert.IsTrue(true);
        }
    }
}
