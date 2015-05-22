using MongoDB.Driver;
using Nancy;
using Nancy.Conventions;
using Nancy.TinyIoc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodash.Core.UI
{
    public class CustomConventionsBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoCContainer existingContainer)
        {
            var repoPath = Path.Combine(Environment.CurrentDirectory, "Repository");

            existingContainer.Register<IMongoDatabase>((container, parameters) => MongoDatabaseProvider.GetDatabase());
            existingContainer.Register<ITestAssembliesRepository>(new FileSystemTestAssembliesRepository(repoPath));
            
            existingContainer.Register<CreateProjectCommand>();
            existingContainer.Register<CreateSuiteCommand>();
        }

        protected override void ApplicationStartup(TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            this.Conventions.ViewLocationConventions.Add((viewName, model, context) =>
            {
                return string.Concat("UI/Views/", viewName);
            });

            this.Conventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("assets", @"UI/Content")
            );
        }
    }
}
