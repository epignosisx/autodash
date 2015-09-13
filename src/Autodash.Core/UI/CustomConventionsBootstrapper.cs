using System.Configuration;
using MongoDB.Driver;
using Nancy;
using Nancy.Conventions;
using Nancy.Json;
using Nancy.TinyIoc;
using System;
using System.IO;
using NLog;

namespace Autodash.Core.UI
{
    public class CustomConventionsBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoCContainer existingContainer)
        {
            var repoPath = ConfigurationManager.AppSettings["RepositoryPath"] ?? Path.Combine(Environment.CurrentDirectory, "Repository");
            var loggerProvider = new DefaultLoggerProvider();
            LogManager.ThrowExceptions = true;
            existingContainer.Register<ILoggerProvider>(loggerProvider);

            var websiteRoot = new Uri(ConfigurationManager.AppSettings["WebsiteRoot"]);
            var emailNotifier = new SuiteRunCompletedEmailNotifier(
                MongoDatabaseProvider.GetDatabase, 
                loggerProvider, websiteRoot, 
                Path.Combine(Environment.CurrentDirectory, "UI\\Content\\suite-complete-tmpl.html")
            );
            existingContainer.Register<ISuiteRunCompletedNotifier>(emailNotifier);

            existingContainer.Register<IMongoDatabase>((container, parameters) => MongoDatabaseProvider.GetDatabase());
            existingContainer.Register<ITestAssembliesRepository>(new FileSystemTestAssembliesRepository(repoPath));
            existingContainer.Register<ITestSuiteUnitTestDiscoverer, DefaultTestSuiteUnitTestDiscoverer>().AsSingleton();
            existingContainer.Register<ITestSuiteFileExplorer>(new XmlOnlyTestSuiteFileExplorer());

            existingContainer.Register<IGridConsoleScraper, DefaultGridConsoleScraper>().AsSingleton();
            
            existingContainer.Register<ISuiteRunSchedulerRepository, DefaultSuiteRunSchedulerRepository>().AsSingleton();
            existingContainer.Register<ISuiteRunner, ParallelSuiteRunner>().AsSingleton();
            existingContainer.Register<ISuiteRunScheduler, ParallelSuiteRunScheduler>().AsSingleton();

            existingContainer.Register<ISeleniumGridBrowserProvider, StaticSeleniumGridBrowserProvider>().AsSingleton();

            existingContainer.Register<CreateProjectCommand>();
            existingContainer.Register<CreateSuiteCommand>();
            existingContainer.Register<UpdateGridCommand>();
            existingContainer.Register<UpdateEmailCommand>();

            TestRunnerPreProcessorProvider.Add(new ApplyTestSettingsPreProcessor(loggerProvider));

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
