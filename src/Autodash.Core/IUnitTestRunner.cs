using System.Threading.Tasks;
namespace Autodash.Core
{
    public interface IUnitTestRunner
    {
        Task<UnitTestResult> Run(UnitTestInfo unitTest, UnitTestCollection testCollection, TestSuiteConfiguration config);
    }
}