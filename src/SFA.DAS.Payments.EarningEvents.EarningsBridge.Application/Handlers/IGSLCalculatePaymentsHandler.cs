using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers;

public interface IGslCalculatePaymentsHandler
{
    Task HandleGslCalculatePaymentsMessage(CalculateGrowthAndSkillsPayments message);
}