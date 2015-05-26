using System.Threading.Tasks;
namespace Autodash.Core
{
    public interface ISuiteRunScheduler
    {
        Task Start();
        Task Schedule(TestSuite suite);
    }
}