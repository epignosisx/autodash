using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodash.Core
{
    public class MSTestRunner : IUnitTestRunner
    {
        public UnitTestResult Run(UnitTestInfo unitTest, UnitTestCollection testCollection, string[] browsers)
        {
            if (unitTest == null)
                throw new ArgumentNullException("unitTest");
            if (testCollection == null)
                throw new ArgumentNullException("testCollection");
            if (browsers == null)
                throw new ArgumentNullException("browsers");
            if (browsers.Length == 0)
                throw new ArgumentException("At least one browser must be specified", "browsers");

            Parallel.ForEach(browsers, browser => { });

            throw new NotImplementedException();
        }
    }
}
