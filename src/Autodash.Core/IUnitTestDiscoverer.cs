namespace Autodash.Core
{
    public interface IUnitTestDiscoverer
    {
        UnitTestCollection DiscoverTests(string assemblyPath);
    }
}