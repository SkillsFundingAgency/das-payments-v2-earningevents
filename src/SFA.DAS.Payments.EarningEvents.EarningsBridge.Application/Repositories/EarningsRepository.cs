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
        private readonly IRepositoryService _repositoryService;
        private readonly ILogger<EarningsRepository> _logger;
        
        public EarningsRepository(IEarningsDataContext earningsDataContext, IRepositoryService repositoryService, ILogger<EarningsRepository> logger)
        {
            _earningsDataContext = earningsDataContext;
            _repositoryService = repositoryService;
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

        public async Task<bool> CheckEarningsAreLatest(CalculateGrowthAndSkillsPayments message)
        {
            try
            {
                var ukPrn = message.UKPRN;
                var uln = message.Learner.ULN;
                var courseCode = message.Training.CourseCode;
                var messageEarningsId = message.EarningsId;

                var earnings = _earningsDataContext.GrowthAndSkillsEarnings
                    .Where(x => x.UKPRN == ukPrn && x.LearnerUln == uln && x.CourseCode == courseCode).ToList();

                // If there are no earnings for the given UKPRN, ULN, and CourseCode, we can consider the current earnings as the latest.
                if (earnings.Count == 0)
                {
                    return true;
                }

                foreach (var earning in earnings)
                {
                    var tableEarningsId = earning.EarningsId;
                    var isLatest = _repositoryService.CheckEarningsAreLatest(messageTimestamp: messageEarningsId,
                        tableEntryTimestamp: tableEarningsId);

                    if (!isLatest)
                    {
                        return false;
                    }
                }

                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while querying GrowthAndSkills data. Exception: {ex.Message}");
            }

            return false;
        }
    }
}
