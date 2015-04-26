using FluentValidation;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Autodash.Core.Tests
{
    public class CreateProjectCommandTests
    {
        [Fact]
        public async Task ValidProjectIsCreated()
        {
            //arrange
            var db = await MongoTestDbProvider.GetDatabase();
            var cmd = new CreateProjectCommand();
            Project project = new Project();
            project.Name = "Test Project";

            //act
            await cmd.ExecuteAsync(db, project);
            
            //assert
            Assert.NotNull(project.Id);
            Assert.NotEqual(project.Id.Length, 0);
        }

        [Fact]
        public async Task ProjectWithMissingNameThrowsValidationException()
        {
            //arrange
            var db = await MongoTestDbProvider.GetDatabase();
            var cmd = new CreateProjectCommand();
            Project project = new Project();
            project.Name = null;

            //act/assert
            await Assert.ThrowsAsync<ValidationException>(() => cmd.ExecuteAsync(db, project));
        }
    }
}
