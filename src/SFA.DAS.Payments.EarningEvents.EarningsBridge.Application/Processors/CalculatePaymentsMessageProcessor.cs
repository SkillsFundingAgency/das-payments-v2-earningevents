using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors;

public class CalculatePaymentsMessageProcessor : IIncomingEarningsMessageProcessor
{
    private readonly IGSLCalculatePaymentsHandler _handler;

    public CalculatePaymentsMessageProcessor(IGSLCalculatePaymentsHandler handler)
    {
        _handler = handler;
    }

    public async Task Process(BinaryData messageBody)
    {
        var message = messageBody.ToObjectFromJson<CalculateGrowthAndSkillsPayments>()
            ?? throw new InvalidOperationException("The calculate payments message body is empty.");
        await _handler.HandleGslCalculatePaymentsMessage(message);
    }
}
