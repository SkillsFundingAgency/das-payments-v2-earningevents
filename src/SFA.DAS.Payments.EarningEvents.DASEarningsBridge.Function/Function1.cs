using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Payments.EarningEvents.DASEarningsSubscriber;

//Theory behind queue system
// https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview

// Explains the file contents
// https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?tabs=python-v2%2Cisolated-process%2Cnodejs-v4%2Cqueue%2Cextensionv5&pivots=programming-language-csharp

// Explains how to test it locally via azure set up
//https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-to-event-grid-integration-function


//Function 1 name could be ServiceBusDASEarnings[Message?]Subscriber
public class Function1
{
    private readonly ILogger<Function1> _logger;

    public Function1(ILogger<Function1> logger)
    {
        _logger = logger;
    }

    [Function(nameof(Function1))]
    public async Task Run(
        [ServiceBusTrigger("myqueue", Connection = "")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        // Complete the message
        await messageActions.CompleteMessageAsync(message);
        //tells Azure Service Bus "I've successfully processed this message — remove it from the queue so it won't be delivered again."
    }
}