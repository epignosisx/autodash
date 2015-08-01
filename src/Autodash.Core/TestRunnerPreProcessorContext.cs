namespace Autodash.Core
{
    public class TestRunnerPreProcessorContext
    {
        public string TestDirectory { get; set; }
        public TestSuiteConfiguration TestSuiteConfiguration { get; set; }
        public UnitTestInfo UnitTestInfo { get; set; }
        public GridNodeBrowserInfo  NodeBrowser { get; set; }
        public SeleniumGridConfiguration GridConfiguration { get; set; }
    }
}