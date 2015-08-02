using System;
using System.Diagnostics;
using System.Threading;

namespace Autodash.Core
{
    public static class ObsEx
    {
        public static void DebugWriteLine(object line)
        {
            Debug.WriteLine(Thread.CurrentThread.ManagedThreadId + ": " + line);
        }
    }
}
