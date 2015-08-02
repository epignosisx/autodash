using System.Threading.Tasks;
namespace Autodash.Core
{
    public interface IUnitTestRunner
    {
        string TestRunnerName { get; }
        Task<UnitTestBrowserResult> Run(TestRunContext context);
    }
}