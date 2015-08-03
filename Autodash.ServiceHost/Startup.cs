using Owin;
namespace Autodash.ServiceHost
{

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseNancy();
        }
    }
}
