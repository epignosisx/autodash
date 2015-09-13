using System.Linq;
using Autodash.Core.UI.Models;
using FluentValidation;
using MongoDB.Bson;
using MongoDB.Driver;
using Nancy;
using Nancy.ModelBinding;
using Nancy.TinyIoc;

namespace Autodash.Core.UI.Modules
{
    public class EmailModule : NancyModule
    {
        public EmailModule(TinyIoCContainer container)
        {
            Get["/email", true] = async (parameters, ct) =>
            {
                var database = container.Resolve<IMongoDatabase>();
                var filter = new BsonDocument();//get all
                EmailConfiguration config = await database.GetCollection<EmailConfiguration>("EmailConfiguration")
                                                                 .FindAsync(filter)
                                                                 .ToFirstOrDefaultAsync();
                config = config ?? new EmailConfiguration();
                var vm = new EmailConfigVm
                {
                    SmtpServer = config.SmtpServer,
                    Port = config.Port,
                    FromEmail = config.FromEmail
                };

                return View["EmailConfig", vm];
            };

            Post["/email", true] = async (parameters, ct) =>
            {
                var vm = this.Bind<EmailConfigVm>();

                var config = new EmailConfiguration
                {
                    SmtpServer = vm.SmtpServer,
                    Port = vm.Port,
                    FromEmail = vm.FromEmail
                };

                var cmd = container.Resolve<UpdateEmailCommand>();

                try
                {
                    await cmd.ExecuteAsync(config);
                }
                catch (ValidationException ex)
                {
                    vm.Errors = ex.Errors.ToArray();
                    return View["EmailConfig", vm];
                }

                return Response.AsRedirect("/email");
            };
        }
    }
}