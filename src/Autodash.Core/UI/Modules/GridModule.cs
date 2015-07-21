using Autodash.Core.UI.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Nancy;
using Nancy.TinyIoc;

namespace Autodash.Core.UI.Modules
{
    public class GridModule : NancyModule
    {
        public GridModule(TinyIoCContainer container)
        {
            Get["/grid", true] = async (parameters, ct) =>
            {
                var database = container.Resolve<IMongoDatabase>();
                var filter = new BsonDocument();//get all
                SeleniumGridConfiguration config = await database.GetCollection<SeleniumGridConfiguration>("SeleniumGridConfiguration")
                                                                 .FindAsync(filter)
                                                                 .ToFirstOrDefaultAsync();

                GridConfigVm vm = new GridConfigVm();
                if (config != null)
                {
                    vm.HubUrl = config.HubUrl;
                }

                return View["GridConfig", vm];
            };
        }
    }
}