using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services
{
    // ReSharper disable once InconsistentNaming
    public interface IGSLEarningsService
    {
        bool CheckEarningsAreLatest(List<GrowthAndSkillsEarningModel> earnings, Guid messageEarningsId);
    }
}
