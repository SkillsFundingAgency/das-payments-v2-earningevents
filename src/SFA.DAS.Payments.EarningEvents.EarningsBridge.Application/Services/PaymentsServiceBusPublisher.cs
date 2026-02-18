using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services
{
    public class PaymentsServiceBusPublisher : IPaymentsServiceBusPublisher
    {
        private const string TopicName = "bundle-1";

        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;

        public PaymentsServiceBusPublisher(string serviceBusConnectionString)
        {
            _client = new ServiceBusClient(serviceBusConnectionString);
            _sender = _client.CreateSender(TopicName);
        }

        public async Task Publish<T>(T message)
        {
            var messageId = Guid.NewGuid().ToString("N"); // strip dashes

            var json = JsonSerializer.Serialize(message);

            var serviceBusMessage = new ServiceBusMessage(json)
            {
                MessageId = messageId,
                ContentType = "application/json",
                Subject = typeof(T).Name,
                ApplicationProperties =
                {
                    // NServiceBus compatibility headers
                    ["NServiceBus.EnclosedMessageTypes"] = $"{typeof(T).FullName}, {typeof(T).Assembly.GetName().Name}",
                    ["messageType"] = typeof(T).Name
                }
            };

            await _sender.SendMessageAsync(serviceBusMessage);
        }
    }
}
