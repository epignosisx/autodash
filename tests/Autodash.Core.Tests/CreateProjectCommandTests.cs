using FluentValidation;
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
            var cmd = new CreateProjectCommand(db, new FakeLoggerProvider());
            Project project = new Project();
            project.Name = "Test Project";

            //act
            await cmd.ExecuteAsync(project);
            
            //assert
            Assert.NotNull(project.Id);
            Assert.NotEqual(project.Id.Length, 0);
        }

        [Fact]
        public async Task ProjectWithMissingNameThrowsValidationException()
        {
            //arrange
            var db = await MongoTestDbProvider.GetDatabase();
            var cmd = new CreateProjectCommand(db, new FakeLoggerProvider());
            Project project = new Project();
            project.Name = null;

            //act/assert
            await Assert.ThrowsAsync<ValidationException>(() => cmd.ExecuteAsync(project));
        }

        [Fact]
        public async Task ProjectWithInvalidEmailAddressThrowsValidationException()
        {
            //arrange
            var db = await MongoTestDbProvider.GetDatabase();
            var cmd = new CreateProjectCommand(db, new FakeLoggerProvider());
            Project project = new Project();
            project.Name = "name";
            project.Description = "descrition";
            project.MemberEmails = new[] {"invalidemail"};

            //act/assert
            await Assert.ThrowsAsync<ValidationException>(() => cmd.ExecuteAsync(project));
        }

        [Fact]
        public async Task ProjectWithValidEmailAddressIsCreated()
        {
            //arrange
            var db = await MongoTestDbProvider.GetDatabase();
            var cmd = new CreateProjectCommand(db, new FakeLoggerProvider());
            Project project = new Project();
            project.Name = "name";
            project.Description = "descrition";
            project.MemberEmails = new[] { "anc@test.com" };

            //act
            await cmd.ExecuteAsync(project);

            //assert
            Assert.NotNull(project.Id);
            Assert.NotEqual(project.Id.Length, 0);
        }
    }
}
