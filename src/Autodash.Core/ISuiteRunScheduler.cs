using System.Threading.Tasks;
namespace Autodash.Core
{
    public interface ISuiteRunScheduler
    {
        Task Start();
        void Schedule(TestSuite suite);
    }
}