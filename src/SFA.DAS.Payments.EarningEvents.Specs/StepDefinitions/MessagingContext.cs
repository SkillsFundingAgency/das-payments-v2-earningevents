using NServiceBus;

namespace SFA.DAS.Payments.EarningEvents.Specs.StepDefinitions
{
    public class MessagingContext
    {
        private IEndpointInstance endpointInstance;

        public MessagingContext()
        {
            endpointInstance = TestRunBindings.endpoint;
        }

        public async Task Send<T>(string messageJson)
        {
            var message = System.Text.Json.JsonSerializer.Deserialize<T>(messageJson);
            await endpointInstance.Send("sfa-das-payments-collectionperiod", message);
        }
    }
}