using System.Configuration;
using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
using System.Text;
using FluentValidation;
using Xunit;
using Xunit.Abstractions;

namespace Autodash.Core.Tests
{
    public class ParallelSuiteRunSchedulerTests
    {
        [Fact]
        public void Foo()
        {
             var scraper = new DefaultGridConsoleScraper();
            //var obs = Observable.Create<GridNodeBrowserInfo>((observer) =>
            //{
            //    Timer timer = new Timer(async (state) =>
            //    {
            //        try
            //        {
            //            var nodes = await scraper.GetAvailableNodesInfoAsync(new Uri("http://alexappvm.cloudapp.net:4444/grid/console"));
            //            foreach (var node in nodes)
            //            {
            //                observer.OnNext(node);
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            observer.OnError(ex);
            //        }
            //    }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            //    return Disposable.Create(timer.Dispose);
            //});

            //var obs = Observable.Interval(TimeSpan.FromSeconds(5))
            //    .SkipWhile(n => )
            //    .SelectMany(n => scraper.GetAvailableNodesInfoAsync(new Uri("http://alexappvm.cloudapp.net:4444/grid/console")))
            //    .SelectMany(n => n);

            //obs.Subscribe(node =>
            //{
            //    var test = matcher.FindTest(node);
            //    var result = runner.Run(test);
            //});


            //Thread.Sleep(TimeSpan.FromSeconds(30));
        }
    }
}
