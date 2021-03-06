﻿using Autodash.Core.UI.Models;
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
        public ProjectsModule(TinyIoCContainer container)
        {
            Get["/", true] = async (x, ct) => {
                var database = container.Resolve<IMongoDatabase>();
                var filter = new BsonDocument();//get all
                var results = await database.GetCollection<Project>("Project").FindAsync(filter);

                var projects = new List<Project>();
                while (await results.MoveNextAsync())
                {
                    projects.AddRange(results.Current.ToList());
                }

                var projectRuns = new List<ProjectSuiteRunVm>(projects.Count);
                foreach (var project in projects)
                {
                    var runs = await database.GetSuiteRunsByProjectIdAsync(project.Id, take: 10);
                    projectRuns.Add(new ProjectSuiteRunVm
                    {
                        Project = project,
                        SuiteRuns = runs.Where(n => n.Result != null).Select(n => new ProjectSuiteRunDetail { DurationMinutes = n.DurationMinutes, Outcome = n.Result.Outcome }).ToList()
                    });
                }

                var vm = new ProjectsVm
                {
                    Projects = projectRuns
                };

                return View["Projects", vm];
            };

            Get["/projects/{id}", true] = async (x, ct) =>
            {
                string projectId = x.id;
                var database = container.Resolve<IMongoDatabase>();

                Project project = await database.GetProjectByIdAsync(projectId);
                List<TestSuite> suites = await database.GetSuitesByProjectIdAsync(projectId);
                var suiteVms = new List<ProjectTestSuiteVm>(suites.Count);
                foreach (var suite in suites)
                {
                    var suiteRuns = await database.GetSuiteRunsBySuiteIdAsync(suite.Id, take: 10);
                    var suiteVm = new ProjectTestSuiteVm
                    {
                        Suite = suite,
                        LastSuiteRuns = suiteRuns
                    };
                    suiteVms.Add(suiteVm);
                }

                var vm = new ProjectDetailsVm();
                vm.Project = project;
                vm.Suites = suiteVms;

                return View["ProjectDetails", vm];
            };

            Get["/projects/create"] = _ =>
            {
                var vm = new CreateProjectVm();
                return View["CreateProject", vm];
            };

            Get["/projects/{id}/edit", true] = async (x, ct) =>
            {
                string projectId = x.id;
                var db = container.Resolve<IMongoDatabase>();
                var query = Builders<Project>.Filter.Eq(n => n.Id, projectId);
                var project = await db.GetCollection<Project>("Project").FindAsync(query).ToFirstOrDefaultAsync();
                var vm = new EditProjectVm
                {
                    Id = project.Id,
                    Description = project.Description,
                    ProjectName = project.Name,
                    MemberEmails = string.Join(", ", project.MemberEmails ?? Enumerable.Empty<string>())
                };

                return View["EditProject", vm];
            };

            Post["/projects/create", true] = async (x, ct) =>
            {
                var vm = this.Bind<CreateProjectVm>();
                
                var project = new Project{
                    Name = vm.ProjectName,
                    Description = vm.Description,
                    MemberEmails = vm.GetIndividualMemberEmails().ToArray()
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

            Post["/projects/update", true] = async (x, ct) =>
            {
                var vm = this.Bind<EditProjectVm>();

                var project = new Project
                {
                    Id = vm.Id,
                    Name = vm.ProjectName,
                    Description = vm.Description,
                    MemberEmails = vm.GetIndividualMemberEmails().ToArray()
                };

                var cmd = container.Resolve<UpdateProjectCommand>();

                try
                {
                    await cmd.ExecuteAsync(project);
                }
                catch (ValidationException ex)
                {
                    vm.Errors = ex.Errors.ToArray();
                    return View["EditProject", vm];
                }

                return Response.AsRedirect(string.Format("/projects/{0}", project.Id));
            };

            Post["/projects/delete", true] = async (x, ct) =>
            {
                string id = Request.Form.id;
                var database = container.Resolve<IMongoDatabase>();
                await database.DeleteProjectAsync(id);
                return Response.AsRedirect("/");
            };
        }
    }

}
