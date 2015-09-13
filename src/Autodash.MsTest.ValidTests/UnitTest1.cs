using System;
using System.Threading;
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
            Console.WriteLine("Running UnitTest1.SuccessTest");
            Thread.Sleep(TimeSpan.FromSeconds(5));
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void AnotherSuccessTest()
        {
            Console.WriteLine("Running UnitTest1.AnotherSuccessTest");
            Thread.Sleep(TimeSpan.FromSeconds(5));
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void FailTest()
        {
            Console.WriteLine("Running UnitTest1.FailTest");
            Thread.Sleep(TimeSpan.FromSeconds(5));
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void InconclusiveTest()
        {
            Console.WriteLine("Running UnitTest1.InconclusiveTest");
            Thread.Sleep(TimeSpan.FromSeconds(5));
            Assert.Inconclusive("this test is inconclusive");
        }
    }
}
