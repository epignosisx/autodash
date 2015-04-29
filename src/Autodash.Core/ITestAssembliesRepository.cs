namespace Autodash.Core
{
    public interface ITestAssembliesRepository
    {
        void MoveToTestSuite(TestSuite suite, string currentLocation);
    }
}