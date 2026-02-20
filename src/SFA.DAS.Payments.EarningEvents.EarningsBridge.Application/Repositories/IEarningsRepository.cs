using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;

public interface IEarningsRepository
{
    public void SaveEarnings(GrowthAndSkillsEarningModel growthAndSkillsEarningModel);
}