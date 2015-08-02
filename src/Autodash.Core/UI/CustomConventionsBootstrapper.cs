using System.Configuration;
using MongoDB.Driver;
using Nancy;
using Nancy.Conventions;
using Nancy.Json;
using Nancy.TinyIoc;
using System;
using System.IO;
using NLog;
using NLog.Targets;

namespace Autodash.Core.UI
{
    public class CustomConventionsBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoCContainer existingContainer)
        {
            var repoPath = ConfigurationManager.AppSettings["RepositoryPath"] ?? Path.Combine(Environment.CurrentDirectory, "Repository");

            LogManager.ThrowExceptions = true;
            existingContainer.Register<ILoggerProvider>(new DefaultLoggerProvider());

            existingContainer.Register<IMongoDatabase>((container, parameters) => MongoDatabaseProvider.GetDatabase());
            existingContainer.Register<ITestAssembliesRepository>(new FileSystemTestAssembliesRepository(repoPath));
            existingContainer.Register<ITestSuiteUnitTestDiscoverer, DefaultTestSuiteUnitTestDiscoverer>().AsSingleton();
            existingContainer.Register<ITestSuiteFileExplorer>(new XmlOnlyTestSuiteFileExplorer());

            existingContainer.Register<IGridConsoleScraper, DefaultGridConsoleScraper>().AsSingleton();
            
            existingContainer.Register<ISuiteRunSchedulerRepository, DefaultSuiteRunSchedulerRepository>().AsSingleton();
            existingContainer.Register<ISuiteRunner, ParallelSuiteRunner>().AsSingleton();
            existingContainer.Register<ISuiteRunScheduler, ParallelSuiteRunScheduler>().AsSingleton();

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
