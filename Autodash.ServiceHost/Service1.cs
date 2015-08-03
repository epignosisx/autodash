using System.Configuration;
using Microsoft.Owin.Hosting;
using System;
using System.ServiceProcess;

namespace Autodash.ServiceHost
{
    public partial class Service1 : ServiceBase
    {
        private IDisposable _appDisposable;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            string port = ConfigurationManager.AppSettings["Port"] ?? "8080";
            var url = "http://+:" + port;
            _appDisposable = WebApp.Start<Startup>(url);
        }

        protected override void OnStop()
        {
            _appDisposable.Dispose();
        }
    }
}
