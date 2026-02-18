using System.Net;
using Azure;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Infrastructure.Configuration;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
// ReSharper disable InconsistentNaming

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Function
{
    
    public class DASEarningsReceiver
    {
        private readonly ILogger<DASEarningsReceiver> _logger;
        private readonly IGSLCalculatePaymentsHandler _gSLCalculatePaymentsHandler;
        
        public DASEarningsReceiver(ILogger<DASEarningsReceiver> logger, IGSLCalculatePaymentsHandler gSLCalculatePaymentsHandler)
        {
            _logger = logger;
            _gSLCalculatePaymentsHandler = gSLCalculatePaymentsHandler;
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

            var growthAndSkillsPaymentsMessage = message.Body.ToObjectFromJson<CalculateGrowthAndSkillsPayments>();
            _gSLCalculatePaymentsHandler.HandleGslCalculatePaymentsMessage(growthAndSkillsPaymentsMessage);

            await messageActions.CompleteMessageAsync(message);
        }

        
    }
}

