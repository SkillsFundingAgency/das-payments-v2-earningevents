using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;

public interface IEarningsRepository
{
    Task SaveEarnings(GrowthAndSkillsEarningModel growthAndSkillsEarningModel);
    List<GrowthAndSkillsEarningModel> GetGrowthAndSkillsEarnings(CalculateGrowthAndSkillsPayments message);
}