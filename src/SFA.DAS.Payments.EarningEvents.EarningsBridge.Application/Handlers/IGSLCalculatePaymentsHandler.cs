using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers;

public interface IGSLCalculatePaymentsHandler
{
    public void HandleGslCalculatePaymentsMessage(CalculateGrowthAndSkillsPayments message);
}