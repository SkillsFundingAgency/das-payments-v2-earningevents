namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors;

public interface IIncomingEarningsMessageProcessorFactory
{
    IIncomingEarningsMessageProcessor Create(string enclosedMessageType);
}
