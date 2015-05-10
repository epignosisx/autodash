using System.Diagnostics;
using System.Threading.Tasks;

namespace Autodash.Core
{
    public interface ISuiteRunner
    {
        Task<SuiteRun> Run(SuiteRun run);
    }

    public class UnitTestResult
    {
    }
}