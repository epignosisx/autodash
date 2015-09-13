using System.Collections.Generic;

namespace Autodash.Core
{
    public interface ISuiteRunCompletedNotifier
    {
        void Notify(SuiteRun suiteRun);
    }
}
