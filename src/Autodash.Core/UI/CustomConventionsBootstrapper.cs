using MongoDB.Driver;
using Nancy;
using Nancy.Conventions;
using Nancy.Json;
using Nancy.TinyIoc;
using System;
using System.IO;

namespace Autodash.Core.UI
{
    public class CustomConventionsBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoCContainer existingContainer)
        {
            var repoPath = Path.Combine(Environment.CurrentDirectory, "Repository");

            existingContainer.Register<IMongoDatabase>((container, parameters) => MongoDatabaseProvider.GetDatabase());
            existingContainer.Register<ITestAssembliesRepository>(new FileSystemTestAssembliesRepository(repoPath));
            existingContainer.Register<ITestSuiteUnitTestDiscoverer, DefaultTestSuiteUnitTestDiscoverer>().AsSingleton();
            
            existingContainer.Register<IGridConsoleScraper, DefaultGridConsoleScraper>().AsSingleton();
            
            existingContainer.Register<ISuiteRunSchedulerRepository, DefaultSuiteRunSchedulerRepository>().AsSingleton();
            existingContainer.Register<ISuiteRunner, ParallelSuiteRunner>().AsSingleton();
            existingContainer.Register<ISuiteRunScheduler, ParallelSuiteRunScheduler>().AsSingleton();
            //existingContainer.Register<ISuiteRunner, DefaultSuiteRunner>().AsSingleton();
            //existingContainer.Register<ISuiteRunScheduler, DefaultSuiteRunScheduler>().AsSingleton();
            
            existingContainer.Register<CreateProjectCommand>();
            existingContainer.Register<CreateSuiteCommand>();
            existingContainer.Register<UpdateGridCommand>();

            TestRunnerPreProcessorProvider.Add(new ApplyTestSettingsPreProcessor());

            JsonSettings.MaxJsonLength = int.MaxValue;

            var scheduler = existingContainer.Resolve<ISuiteRunScheduler>();
            scheduler.Start();
        }

        protected override void ApplicationStartup(TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            this.Conventions.ViewLocationConventions.Add((viewName, model, context) => string.Concat("UI/Views/", viewName));

            this.Conventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("assets", @"UI/Content")
            );
        }
    }
}
