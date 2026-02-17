using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.EarningEvents.Data;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Function;

//Function 1 name could be ServiceBusDASEarnings[Message?]Subscriber
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

        
        //_paymentsRepository.getcurrentcollectionperiod
        
        //Deserialize into GSL calculate payments model, object
        CalculateGrowthAndSkillsPayments growthAndSkillsPaymentsMessage = message.Body.ToObjectFromJson<CalculateGrowthAndSkillsPayments>();
        _gSLCalculatePaymentsHandler.HandleGslCalculatePaymentsMessage(growthAndSkillsPaymentsMessage);

        
        // Handler to GSLEarningsMapper - Handler pass in GSLEarningsEvents (test check success) - later check, not sure why this is needed
        // Check ProviderPayments mapper(for the processor) - easiest to check with UnitTest
        // GSLEarningsMapper IEarningEvent Creation
            //Inject Dave's short course earning model for IEarning event
            //Put in the Earning event message
        // IEarningEarningEvent
        await messageActions.CompleteMessageAsync(message);
    }
}