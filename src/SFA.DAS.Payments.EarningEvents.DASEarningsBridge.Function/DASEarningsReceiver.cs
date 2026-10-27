using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Logging;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.Messages.Common;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Function
{
    public class DASEarningsReceiver
    {
        private readonly ILogger<DASEarningsReceiver> _logger;
        private readonly IGSLCalculatePaymentsHandler _gslCalculatePaymentsHandler;

        public DASEarningsReceiver(ILogger<DASEarningsReceiver> logger, IGSLCalculatePaymentsHandler gslCalculatePaymentsHandler)
        {
            _logger = logger;
            _gslCalculatePaymentsHandler = gslCalculatePaymentsHandler;
        }

        [Function(nameof(DASEarningsReceiver))]
        public async Task Run(
            [ServiceBusTrigger("%DASServiceBusQueueName%", Connection = "DASServiceBusConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var growthAndSkillsPaymentsMessage = message.Body.ToObjectFromJson<CalculateGrowthAndSkillsPayments>();

            if (growthAndSkillsPaymentsMessage is null)
            {
                throw new InvalidOperationException($"Service Bus message {message.MessageId} could not be deserialized.");
            }

            _logger.LogEarningsBridgeTrace("EarningsBridge.MessageReceived",
            new
            {
                ExternalEarningsId = growthAndSkillsPaymentsMessage?.EarningsId,
                UKPRN = growthAndSkillsPaymentsMessage?.UKPRN,
                ULN = growthAndSkillsPaymentsMessage?.Learner?.ULN,
                AcademicYear = growthAndSkillsPaymentsMessage?.Earnings?.FirstOrDefault()?.AcademicYear,
                CollectionPeriod = growthAndSkillsPaymentsMessage?.Earnings?
                    .SelectMany(x => x.PricePeriods)
                    .SelectMany(x => x.Periods)
                    .Select(x => x.DeliveryPeriod)
                    .FirstOrDefault()
            });

            await _gslCalculatePaymentsHandler.HandleGslCalculatePaymentsMessage(growthAndSkillsPaymentsMessage);
            await messageActions.CompleteMessageAsync(message);
        }
    }
}