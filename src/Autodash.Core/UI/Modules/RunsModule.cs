using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodash.Core.UI.Models;
using MongoDB.Driver;
using Nancy;
using Nancy.TinyIoc;

namespace Autodash.Core.UI.Modules
{
    public class RunsModule : NancyModule
    {
        public RunsModule(TinyIoCContainer container)
        {
            Get["/runs/{id}/report.html", true] = async (x, ct) =>
            {
                string id = x.id;
                var database = container.Resolve<IMongoDatabase>();
                
                SuiteRun run = await database.GetSuiteRunByIdAsync(id);
                Project project = await database.GetProjectByIdAsync(run.TestSuiteSnapshot.ProjectId);

                SuiteRunDetailsVm vm = new SuiteRunDetailsVm
                {
                    Project = project,
                    SuiteRun = run
                };

                return View["SuiteRunDetails", vm];
            };
        }
    }
}
