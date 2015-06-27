using Autodash.Core.UI.Models;
using MongoDB.Bson;
using MongoDB.Driver;
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
            Get["/", true] = async (x, ct) => {
                var database = container.Resolve<IMongoDatabase>();
                var filter = new BsonDocument();//get all
                var results = await database.GetCollection<Project>("Project").FindAsync(filter);

                List<Project> projects = new List<Project>();
                while (await results.MoveNextAsync())
                {
                    projects.AddRange(results.Current.ToList());
                }

                var vm = new ProjectsVm();
                vm.Projects = projects;

                return View["Projects", vm];
            };

            Get["/{id}", true] = async (x, ct) =>
            {
                string projectId = x.id;
                var database = container.Resolve<IMongoDatabase>();

                var project = await database.GetProjectByIdAsync(projectId);
                var suites = await database.GetSuitesByProjectIdAsync(projectId);
                var suiteRuns = await database.GetSuiteRunsBySuiteIdsAsync(suites.Select(n => n.Id), take: 10);

                ProjectDetailsVm vm = new ProjectDetailsVm();
                vm.Project = project;
                vm.Suites = suites;
                vm.LastSuiteRuns = suiteRuns;

                return View["ProjectDetails", vm];
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
