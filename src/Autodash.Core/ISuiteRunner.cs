using System.Threading;
using System.Threading.Tasks;

namespace Autodash.Core
{
    public interface ISuiteRunner
    {
        Task<SuiteRun> Run(SuiteRun run, CancellationToken cancellationToken);
    }
}