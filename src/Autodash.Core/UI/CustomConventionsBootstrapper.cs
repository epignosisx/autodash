using MongoDB.Driver;
using Nancy;
using Nancy.Conventions;
using Nancy.TinyIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodash.Core.UI
{
    public class CustomConventionsBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoCContainer existingContainer)
        {
            // We don't call base because we don't want autoregister
            // we just register our one known dependency as an application level singleton
            existingContainer.Register<IMongoDatabase>((container, parameters) => MongoDatabaseProvider.GetDatabase());
        }

        protected override void ApplicationStartup(TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
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
