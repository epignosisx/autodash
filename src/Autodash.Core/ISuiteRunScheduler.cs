using System.Threading.Tasks;

namespace Autodash.Core
{
    public interface ISuiteRunScheduler
    {
        Task Start();
        Task<SuiteRun> Schedule(TestSuite suite);
        bool TryCancelRunningSuite(string id);
    }
}