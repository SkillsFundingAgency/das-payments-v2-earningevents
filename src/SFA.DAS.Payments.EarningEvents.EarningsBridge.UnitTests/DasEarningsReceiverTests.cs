    //    using System;
    //using System.Text.Json;
    //using System.Threading.Tasks;
    //using Azure.Messaging.ServiceBus;
    //using Microsoft.Azure.Functions.Worker;
    //using Microsoft.Extensions.Logging;
    //using Microsoft.Extensions.Logging.Abstractions;
    //using Moq;
    //using SFA.DAS.Payments.EarningEvents.EarningsBridge.Function;
    //using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

    //using Xunit;
    //using Xunit.Abstractions;
    //using static System.Runtime.InteropServices.JavaScript.JSType;

    //public class DasEarningsReceiverTests
    //{
    //    // When the subscriber gets a message it should write some logs with message details
    //    // It should turn the message body (JSON) into a CalculateGSLPayments object
    //    // It should tell the service bus that the message was handled (CompleteMessageAsync).

    //    [Fact]
    //    public async Task Run_Calls_CompleteMessageAsync_With_ReceivedMessage()
    //    {
    //        // Initial setup
    //        // Creates a small payload to be read
    //        var payload = new CalculateGSLPayments
    //        {
    //            EarningsId = 1,
    //            UKPRN = 12345
    //        };

    //        // Turn the payload into a JSON string - due to service bus setup
    //        var json = JsonSerializer.Serialize(payload);

    //        // Turns into binary as that's what Azure uses
    //        var body = BinaryData.FromString(json);

    //        //Constructs a fake received message which uses earlier's payload
    //        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
    //            body: body,
    //            messageId: "msg-1",
    //            contentType: "application/json"
    //        );

    //        // Need to look further into this line of code
    //        var messageActionsMock = new Mock<ServiceBusMessageActions>();
    //        // Set 
    //        messageActionsMock
    //            .Setup(x => x.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>()))
    //            .Returns(Task.CompletedTask)
    //            .Verifiable(); // Allows it to check if it's been sent or not

    //        // Fake logger is created, in order to stop real logs being produced?
    //        var loggerMock = new Mock<ILogger<DASEarningsReceiver>>();

    //        // Tests the subscriber
    //        var subscriber = new DASEarningsReceiver(loggerMock.Object);

    //        // Runs the code
    //        await subscriber.Run(message, messageActionsMock.Object);
            
    //        // Checks that completeMessageAsync was run i.e. message was finished and also checks the message id
    //        messageActionsMock.Verify(x =>
    //            x.CompleteMessageAsync(It.Is<ServiceBusReceivedMessage>(m => m.MessageId == "msg-1"), It.IsAny<CancellationToken>()),
    //            Times.Once);
    //    }
    //}
