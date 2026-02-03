using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators
{
    // ReSharper disable once InconsistentNaming
    public interface ICalculateGSLPaymentsValidator
    {
        bool Validate(CalculateGSLPayments command);
    }
}
