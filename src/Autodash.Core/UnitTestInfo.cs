using System;

namespace Autodash.Core
{
    public class UnitTestInfo
    {
        public string TestName { get; private set; }
        public string ShortTestName { get; private set; }
        public string[] TestTags { get; private set; }

        public UnitTestInfo(string testName, string[] testTags)
        {
            if (testName == null) 
                throw new ArgumentNullException("testName");

            TestName = testName;
            TestTags = testTags;

            if (TestName.Contains("."))
                ShortTestName = TestName.Substring(TestName.LastIndexOf(".", System.StringComparison.Ordinal) + 1);
            else
                ShortTestName = TestName;
        }
    }
}