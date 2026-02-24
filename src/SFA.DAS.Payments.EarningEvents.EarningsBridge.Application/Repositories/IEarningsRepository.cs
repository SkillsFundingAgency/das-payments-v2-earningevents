using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;

public interface IEarningsRepository
{
    Task SaveEarnings(GrowthAndSkillsEarningModel growthAndSkillsEarningModel);
}