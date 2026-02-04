using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.EarningEvents.Data;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Function;

//Theory behind queue system
// https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview

// Explains the file contents
// https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?tabs=python-v2%2Cisolated-process%2Cnodejs-v4%2Cqueue%2Cextensionv5&pivots=programming-language-csharp

// Explains how to test it locally via azure set up
//https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-to-event-grid-integration-function


//https://andrewlock.net/using-azure-storage-queues-with-azure-functions-and-queuetrigger/

//Function 1 name could be ServiceBusDASEarnings[Message?]Subscriber
public class DASEarningsReceiver
{
    private readonly ILogger<DASEarningsReceiver> _logger;
    private readonly IgSLCalculatePaymentsHandler _gSLCalculatePaymentsHandler;
    //private readonly IEarningsDataContext;

    public DASEarningsReceiver(ILogger<DASEarningsReceiver> logger, IgSLCalculatePaymentsHandler gSLCalculatePaymentsHandler)
    {
        _logger = logger;
        _gSLCalculatePaymentsHandler = gSLCalculatePaymentsHandler;
        //_earningsRepository = earningsRepository; 
    }

    [Function(nameof(DASEarningsReceiver))]
    public async Task Run(
        [ServiceBusTrigger("DASServiceBusQueueName", Connection = "DASServiceBusConnectionString")]

ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        
        //_paymentsRepository.getcurrentcollectionperiod
        
        //Deserialize into GSL calculate payments model, object
        CalculateGSLPayments gSLPaymentMessage = message.Body.ToObjectFromJson<CalculateGSLPayments>();
        _gSLCalculatePaymentsHandler.HandleGslCalculatePaymentsMessage(gSLPaymentMessage);


        
        
        // Handler to GSLEarningsProcessor - Handler pass in GSLEarningsEvents (test check success) - later check, not sure why this is needed
        // Check ProviderPayments mapper(for the processor) - easiest to check with UnitTest
        // GSLEarningsProcessor IEarningEvent Creation
            //Inject Dave's short course earning model for IEarning event
            //Put in the Earning event message
        // IEarningEarningEvent
        //IoC, Dependency Injection

        //(Would be in Handler file, not called here) Handler to handle incoming calculate GSL payments message
        //Another service (called within) GSLEarningProcessor - Code that is going to do conversion/mapping of the command
        //Mapping class
        //Another service (called within) CollectionPeriodAPI- collection period API check




        await messageActions.CompleteMessageAsync(message);
        //tells Azure Service Bus "I've successfully processed this message — remove it from the queue so it won't be delivered again."
    }
}