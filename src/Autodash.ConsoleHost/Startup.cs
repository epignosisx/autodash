using Owin;

namespace Autodash.ConsoleHost
{

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseNancy();
        }
    }
}
