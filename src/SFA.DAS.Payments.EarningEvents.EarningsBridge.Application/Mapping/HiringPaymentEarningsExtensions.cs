using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping
{
    public static class HiringPaymentEarningsExtensions
    {
        public static bool IsHiringPayment(this EarningType earningType) =>
            earningType == EarningType.FirstEmployerHiringPayment ||
            earningType == EarningType.SecondEmployerHiringPayment;

        public static bool ContainsOnlyHiringPaymentEarnings(this CalculateGrowthAndSkillsPayments message)
        {
            var periods = message.Earnings
                .SelectMany(e => e.PricePeriods)
                .SelectMany(p => p.Periods)
                .ToList();

            return periods.Any() && periods.All(p => p.EarningType.IsHiringPayment());
        }
    }
}
