namespace Autodash.Core
{
    public interface IUnitTestRunner
    {
        UnitTestResult Run(UnitTestInfo unitTest, UnitTestCollection testCollection, TestSuiteConfiguration config);
    }
}