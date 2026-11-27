namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors;

public interface IIncomingEarningsMessageProcessor
{
    Task Process(BinaryData messageBody);
}
