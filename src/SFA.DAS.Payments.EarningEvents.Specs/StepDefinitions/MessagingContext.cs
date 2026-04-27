using NServiceBus;

namespace SFA.DAS.Payments.EarningEvents.Specs.StepDefinitions
{
    public class MessagingContext
    {
        private IEndpointInstance endpointInstance;

        public MessagingContext(IEndpointInstance endpointInstance)
        {
            this.endpointInstance = endpointInstance;
        }

        //public async Task Send<T>(string messageJson)
        //{
        //    var message = System.Text.Json.JsonSerializer.Deserialize<T>(messageJson);
        //    await endpointInstance.Send("sfa-das-payments-collectionperiod", message);
        //}

        public async Task Send<T>(T message)
        {
            await endpointInstance.Send("sfa-das-payments-earningsevents-dasearningsbridge", message);
        }
    }
}