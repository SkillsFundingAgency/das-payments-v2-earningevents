using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping
{
    public class GslApprenticeshipsMapper : GrowthAndSkillsMapper, IGslApprenticeshipsMapper
    {
        public IEnumerable<GSLApprenticeshipEarningsEvent> MapToApprenticeshipEarningEvents(CalculateGrowthAndSkillsPayments source, IEnumerable<CollectionPeriodModel> openCollectionPeriods)
        {
            throw new NotImplementedException();
        }
    }
}
