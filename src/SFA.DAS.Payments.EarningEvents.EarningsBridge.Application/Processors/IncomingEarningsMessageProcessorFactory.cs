using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors;

public class IncomingEarningsMessageProcessorFactory : IIncomingEarningsMessageProcessorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public IncomingEarningsMessageProcessorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IIncomingEarningsMessageProcessor Create(string enclosedMessageType)
    {
        if (string.IsNullOrWhiteSpace(enclosedMessageType))
        {
            throw new InvalidOperationException("The NServiceBus.EnclosedMessageTypes header is missing.");
        }

        var messageTypes = enclosedMessageType
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(type => type.Split(',')[0].Trim())
            .ToHashSet(StringComparer.Ordinal);

        if (messageTypes.Contains(typeof(WithholdPaymentsForLearningEvent).FullName!))
        {
            return _serviceProvider.GetRequiredService<WithholdPaymentsForLearningMessageProcessor>();
        }

        if (messageTypes.Contains(typeof(CalculateGrowthAndSkillsPayments).FullName!))
        {
            return _serviceProvider.GetRequiredService<CalculatePaymentsMessageProcessor>();
        }

        throw new InvalidOperationException($"Unsupported message type: {enclosedMessageType}");
    }
}
