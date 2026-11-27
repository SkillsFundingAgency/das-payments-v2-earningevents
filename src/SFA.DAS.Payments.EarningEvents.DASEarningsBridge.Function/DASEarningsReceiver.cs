using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors;
// ReSharper disable InconsistentNaming

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Function
{

    public class DASEarningsReceiver
    {
        private readonly ILogger<DASEarningsReceiver> _logger;
        private readonly IIncomingEarningsMessageProcessorFactory _processorFactory;

        public DASEarningsReceiver(ILogger<DASEarningsReceiver> logger, IIncomingEarningsMessageProcessorFactory processorFactory)
        {
            _logger = logger;
            _processorFactory = processorFactory;
        }

        [Function(nameof(DASEarningsReceiver))]
        public async Task Run(
            [ServiceBusTrigger("%DASServiceBusQueueName%", Connection = "DASServiceBusConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            if (!message.ApplicationProperties.TryGetValue("NServiceBus.EnclosedMessageTypes", out var enclosedMessageType)
                || enclosedMessageType is not string enclosedMessageTypeHeader)
            {
                throw new InvalidOperationException("The NServiceBus.EnclosedMessageTypes header is missing or invalid.");
            }

            var processor = _processorFactory.Create(enclosedMessageTypeHeader);
            await processor.Process(message.Body);

            await messageActions.CompleteMessageAsync(message);
        }
    }
}

