using System.Collections.Generic;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core.Incentives;

namespace SFA.DAS.Payments.EarningEvents.Domain.Mapping
{
    public interface IHiringIncentiveEarningsProvider
    {
        List<(ApprenticeshipContractType1EarningEvent Event, List<IncentiveEarning> IncentiveEarnings)> GetHiringIncentiveEarnings(List<ApprenticeshipContractType1EarningEvent> candidates);
    }
}
