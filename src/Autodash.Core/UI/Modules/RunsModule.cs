using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
            Get["/runs/{id}/report", true] = async (x, ct) =>
            {
                SuiteRunDetailsVm vm = await GetSuiteRunVm(container, x.id);
                return View["SuiteRunDetails", vm];
            };

            Get["/runs/{id}/report.html", true] = async (x, ct) =>
            {
                SuiteRunDetailsVm vm = await GetSuiteRunVm(container, x.id);
                vm.DownloadMode = true;
                vm.EmbedResources = true;

                string safeName = SafeFileName(vm.SuiteRun.TestSuiteSnapshot.Name);
                string disposition = string.Format("attachment; filename=\"{0:yyyy-MM-dd_HH-mm-ss}_{1}_Report.html\"",
                    vm.SuiteRun.StartedOn,
                    safeName
                );

                return View["SuiteRunDetails", vm].WithHeader("Content-Disposition", disposition);
            };
        }

        private static async Task<SuiteRunDetailsVm> GetSuiteRunVm(TinyIoCContainer container, string id)
        {
            var database = container.Resolve<IMongoDatabase>();

            SuiteRun run = await database.GetSuiteRunByIdAsync(id);
            Project project = await database.GetProjectByIdAsync(run.TestSuiteSnapshot.ProjectId);

            SuiteRunDetailsVm vm = new SuiteRunDetailsVm
            {
                Project = project,
                SuiteRun = run
            };
            return vm;
        }


        private static string SafeFileName(string name)
        {
            var illegalChars = Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars());
            foreach (var illegalChar in illegalChars)
            {
                name = name.Replace(illegalChar.ToString(CultureInfo.InvariantCulture), "");
            }
            return name;
        }
    }
}
