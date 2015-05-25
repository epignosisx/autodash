using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodash.Core.UI.Models
{
    public class TestExplorerVm
    {
        public List<UnitTestCollectionVm> UnitTestCollections { get; set; }
    }

    public class UnitTestCollectionVm
    {
        public string AssemblyName { get; set; }
        public string TestRunnerName { get; set; }
        public List<UnitTestInfoVm> Tests { get; set; }
    }

    public class UnitTestInfoVm
    {
        public string TestName { get; set; }
        public string[] TestTags { get; set; }

        public UnitTestInfoVm(string testName, string[] testTags)
        {
            TestName = testName;
            TestTags = testTags;
        }
        
    }
}
