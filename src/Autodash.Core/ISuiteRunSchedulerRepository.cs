using System.Collections.Generic;
using System.Threading.Tasks;

namespace Autodash.Core
{
    public interface ISuiteRunSchedulerRepository
    {
        Task FailRunningSuitesAsync();
        Task<List<TestSuite>> GetTestSuitesWithScheduleAsync();
        Task<List<SuiteRun>> GetScheduledSuiteRunsAsync();
        Task AddSuiteRunAsync(SuiteRun run);
        Task UpdateSuiteRunAsync(SuiteRun run);
        Task<SeleniumGridConfiguration> GetGridConfigurationAsync();
    }
}