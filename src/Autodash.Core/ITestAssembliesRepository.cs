namespace Autodash.Core
{
    public interface ITestAssembliesRepository
    {
        string MoveToTestSuite(TestSuite suite, string zipLocation);
    }
}