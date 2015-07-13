namespace Autodash.Core
{
    public class UnitTestInfo
    {
        public string TestName { get; private set; }

        public string ShortTestName
        {
            get { return TestName.Substring(TestName.LastIndexOf(".", System.StringComparison.Ordinal) + 1); }
        }

        public string[] TestTags { get; private set; }

        public UnitTestInfo(string testName, string[] testTags)
        {
            TestName = testName;
            TestTags = testTags;
        }
    }
}