using Microsoft.Owin.Hosting;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodash.ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = "http://+:8080";

            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Running on {0}", url);
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }
        }
    }

    public class HomeModule : NancyModule
    {
        public HomeModule() : base("/home")
        {
            Get["/"] = parameters => {
                return "Hello World";
            };
        }
    }
}
