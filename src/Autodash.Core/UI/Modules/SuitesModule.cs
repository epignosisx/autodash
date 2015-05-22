using Autodash.Core.UI.Models;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.ModelBinding;
using Nancy.TinyIoc;
using FluentValidation;
using System.IO;

namespace Autodash.Core.UI.Modules
{
    public class SuitesModule : NancyModule
    {
        public SuitesModule(TinyIoCContainer container)
        {
            Get["/projects/{id}/suites/create"] = parameters => {
                var vm = new CreateSuiteVm
                {
                    ProjectId = parameters.id
                };

                return View["CreateSuite", vm];
            };

            Post["/suites/create", true] = async (c, ct) => {
                var vm = this.Bind<CreateSuiteVm>();
                var zip = Request.Files.FirstOrDefault();

                var tempZipLoc = Path.GetTempFileName();
                using(FileStream fs = new FileStream(tempZipLoc, FileMode.Append))
                {
                    await zip.Value.CopyToAsync(fs);
                }

                var cmd = container.Resolve<CreateSuiteCommand>();

                var suite = new TestSuite { 
                    ProjectId = vm.ProjectId,
                    Name = vm.SuiteName,
                    Configuration = new TestSuiteConfiguration
                    {
                        Browsers = vm.Browsers,
                        RetryAttempts = vm.RetryAttempts,
                        TestTimeout = TimeSpan.FromMinutes(vm.TestTimeoutMinutes)
                    },
                    Schedule = vm.Time.HasValue ? new TestSuiteSchedule {
                        Time = vm.Time.Value,
                        Interval = vm.IntervalHours.HasValue ? 
                            TimeSpan.FromHours(vm.IntervalHours.Value) : 
                            TimeSpan.MinValue
                    } : null
                };

                try
                {
                    await cmd.ExecuteAsync(suite, tempZipLoc);
                }
                catch (ValidationException ex)
                {
                    vm.Errors = ex.Errors.ToArray();
                    return View["CreateSuite", vm];
                }
                finally
                {
                    File.Delete(tempZipLoc);
                }

                return Response.AsRedirect(string.Format("/suites/{0}", suite.Id));
            };

            Get["/suites/{id}"] = parameters => {
                return View["Suite"];
            };
        }
    }
}
