using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.EarningEvents.Data;
using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories
{
    public class EarningsRepository : IEarningsRepository
    {
        private readonly IEarningsDataContext _earningsDataContext;
        private readonly ILogger<EarningsRepository> _logger;
        
        public EarningsRepository(IEarningsDataContext earningsDataContext, ILogger<EarningsRepository> logger)
        {
            _earningsDataContext = earningsDataContext;
            _logger = logger;
        }

        public async Task SaveEarnings(GrowthAndSkillsEarningModel growthAndSkillsEarningModel)
        {
            try
            {
                _earningsDataContext.GrowthAndSkillsEarnings.Add(growthAndSkillsEarningModel);
                await _earningsDataContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while storing earnings to database", ex);
            }
        }

        public async Task<List<GrowthAndSkillsEarningModel>> GetGrowthAndSkillsEarnings(long ukPrn, long uln, string courseCode)
        {
            try
            {
                var results =  await _earningsDataContext.GrowthAndSkillsEarnings
                    .Where(x => x.UKPRN == ukPrn && x.LearnerUln == uln && x.CourseCode == courseCode)
                    .ToListAsync();
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while querying GrowthAndSkills data. Exception: {ex.Message}");
                throw;
            }
        }
    }
}
