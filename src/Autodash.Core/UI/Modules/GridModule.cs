﻿using System;
using System.Linq;
using System.Net;
using Autodash.Core.UI.Models;
using FluentValidation;
using FluentValidation.Results;
using MongoDB.Bson;
using MongoDB.Driver;
using Nancy;
using Nancy.Json;
using Nancy.ModelBinding;
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

                var vm = new GridConfigVm();
                if (config != null)
                {
                    vm.HubUrl = config.HubUrl;
                    vm.MaxParallelTestSuitesRunning = config.MaxParallelTestSuitesRunning;
                    
                    using (var webClient = new WebClient())
                    {
                        try
                        {
                            vm.JsonConfig = await webClient.DownloadStringTaskAsync(new Uri(vm.GridConfig));
                        }
                        catch
                        {
                            vm.Errors = new[]
                            {
                                new ValidationFailure("JsonConfig", "Could not connect to grid. This could be because the grid is down or cannot be reached.")
                            };
                        }
                    }
                }

                return View["GridConfig", vm];
            };

            Post["/grid", true] = async (parameters, ct) =>
            {
                var vm = this.Bind<GridConfigVm>();
                
                var config = new SeleniumGridConfiguration
                {
                    HubUrl = vm.HubUrl,
                    MaxParallelTestSuitesRunning = vm.MaxParallelTestSuitesRunning
                };

                var cmd = container.Resolve<UpdateGridCommand>();

                try
                {
                    await cmd.ExecuteAsync(config);
                }
                catch (ValidationException ex)
                {
                    vm.Errors = ex.Errors.ToArray();
                    return View["GridConfig", vm];
                }

                return Response.AsRedirect("/grid");
            };
        }
    }
}