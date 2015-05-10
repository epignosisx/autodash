namespace Autodash.Core
{
    public class UnitTestInfo
    {
        public string TestName { get; private set; }
        public string[] TestTags { get; private set; }

        public UnitTestInfo(string testName, string[] testTags)
        {
            TestName = testName;
            TestTags = testTags;
        }
    }
}