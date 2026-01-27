using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Messages.Events;

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

    public DASEarningsReceiver(ILogger<DASEarningsReceiver> logger)
    {
        _logger = logger;
    }

    [Function(nameof(DASEarningsReceiver))]
    public async Task Run(
        [ServiceBusTrigger("myqueue", Connection = "placeholder")]

ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);


        //Deserialize into GSL calculate payments model, object
        CalculateGSLPayments content = message.Body.ToObjectFromJson<CalculateGSLPayments>();
        _logger.LogInformation("OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
        Console.WriteLine(content);
        // Handler called - pass in the message, in the handler call the object a message or command
        // Within handler calculateGSLPayments 
        Console.WriteLine("Content has been made");

        await messageActions.CompleteMessageAsync(message);
        //tells Azure Service Bus "I've successfully processed this message — remove it from the queue so it won't be delivered again."
    }
}