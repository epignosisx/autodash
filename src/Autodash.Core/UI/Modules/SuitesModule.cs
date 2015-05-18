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

namespace Autodash.Core.UI.Modules
{
    public class SuitesModule : NancyModule
    {
        public SuitesModule()
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

                var cmd = TinyIoCContainer.Current.Resolve<CreateSuiteCommand>();

                var suite = new TestSuite { 
                    ProjectId = vm.ProjectId,
                    Name = vm.SuiteName,
                };

                try
                {
                    await cmd.ExecuteAsync(suite, "");
                }
                catch (ValidationException ex)
                {
                    vm.Errors = ex.Errors.ToArray();
                    return View["CreateSuite", vm];
                }

                return Response.AsRedirect(string.Format("/suites/{0}", suite.Id));
            };

            Get["/suites/{id}"] = parameters => {
                return View["Suite"];
            };
        }
    }
}
