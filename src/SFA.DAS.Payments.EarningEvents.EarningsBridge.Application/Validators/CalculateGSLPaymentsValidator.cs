using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators
{
    public static class CalculateGSLPaymentsValidator
    {
        public static bool Validate(CalculateGSLPayments command)
        {
            return true;
        }
    }
}
