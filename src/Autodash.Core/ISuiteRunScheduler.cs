namespace Autodash.Core
{
    public interface ISuiteRunScheduler
    {
        void Start();
        void Schedule(TestSuite suite);
    }
}