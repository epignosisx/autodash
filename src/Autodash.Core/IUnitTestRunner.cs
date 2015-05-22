using System.Threading.Tasks;
namespace Autodash.Core
{
    public interface IUnitTestRunner
    {
        string TestRunnerName { get; }
        Task<UnitTestResult> Run(UnitTestInfo unitTest, UnitTestCollection testCollection, TestSuiteConfiguration config);
    }
}