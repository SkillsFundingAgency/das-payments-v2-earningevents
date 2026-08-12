using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping
{
    public interface IGSLApprenticeshipsMapper
    {
        IEnumerable<GSLApprenticeshipEarningsEvent> MapToApprenticeshipEarningEvents(CalculateGrowthAndSkillsPayments source, IEnumerable<CollectionPeriodModel> openCollectionPeriods);
    }
}
