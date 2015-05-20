using Autodash.Core.UI.Models;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.ModelBinding;
using FluentValidation;
using Nancy.TinyIoc;

namespace Autodash.Core.UI.Modules
{
    public class ProjectsModule : NancyModule
    {
        public ProjectsModule(TinyIoCContainer container) : base("/projects")
        {
            Get["/"] = _ => {
                return View["Projects"];
            };

            Get["/create"] = _ => {
                var vm = new CreateProjectVm();
                return View["CreateProject", vm];
            };

            Post["/create", true] = async (x, ct) => {
                var vm = this.Bind<CreateProjectVm>();
                
                var project = new Project{
                    Name = vm.ProjectName,
                    Description = vm.Description
                };

                var cmd = container.Resolve<CreateProjectCommand>();

                try
                {
                    await cmd.ExecuteAsync(project);
                }
                catch (ValidationException ex)
                {
                    vm.Errors = ex.Errors.ToArray();
                    return View["CreateProject", vm];
                }

                return Response.AsRedirect(string.Format("/projects/{0}/suites/create", project.Id));
            };
        }
    }
}
