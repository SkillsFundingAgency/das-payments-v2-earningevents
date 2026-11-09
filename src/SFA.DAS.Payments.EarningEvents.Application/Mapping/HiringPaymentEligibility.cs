using System;
using SFA.DAS.Payments.EarningEvents.Messages.Events;

namespace SFA.DAS.Payments.EarningEvents.Application.Mapping
{
    public static class HiringPaymentEligibility
    {
        public static readonly DateTime StartDate = new DateTime(2026, 10, 1);
        public const int AgeLimit = 25;

        public static bool MeetsStartDateAndAge(ApprenticeshipContractType1EarningEvent earningEvent)
        {
            return earningEvent.StartDate >= StartDate
                   && earningEvent.AgeAtStartOfLearning.HasValue
                   && earningEvent.AgeAtStartOfLearning.Value < AgeLimit;
        }
    }
}
