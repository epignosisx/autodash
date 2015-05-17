using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodash.Core.UI.Modules
{
    public class ProjectsModule : NancyModule
    {
        public ProjectsModule() : base("/projects")
        {
            Get["/"] = parameters => {
                return View["Projects"];
            };
        }
    }
}
