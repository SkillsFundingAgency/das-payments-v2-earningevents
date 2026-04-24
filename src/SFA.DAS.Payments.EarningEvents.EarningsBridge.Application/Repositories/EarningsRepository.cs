using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.EarningEvents.Data;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using UUIDNext.Tools;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories
{
    public class EarningsRepository : IEarningsRepository
    {
        private readonly IEarningsDataContext _earningsDataContext;
        private readonly IGSLService _repositoryService;
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

        public List<GrowthAndSkillsEarningModel> GetGrowthAndSkillsEarnings(CalculateGrowthAndSkillsPayments message)
        {
            try
            {
                var ukPrn = message.UKPRN;
                var uln = message.Learner.ULN;
                var courseCode = message.Training.CourseCode;

                return _earningsDataContext.GrowthAndSkillsEarnings
                    .Where(x => x.UKPRN == ukPrn && x.LearnerUln == uln && x.CourseCode == courseCode).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while querying GrowthAndSkills data. Exception: {ex.Message}");
                throw;
            }
        }
    }
}
