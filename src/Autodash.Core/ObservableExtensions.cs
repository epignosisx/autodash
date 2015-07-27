using System;
using System.Diagnostics;
using System.Threading;

namespace Autodash.Core
{
    public static class ObservableExtensions
    {
        public static IObservable<TSource> DebugWriteLine<TSource>(this IObservable<TSource> source, string line)
        {
            Debug.WriteLine(Thread.CurrentThread.ManagedThreadId + ": " + line);
            return source;
        }
    }
}
